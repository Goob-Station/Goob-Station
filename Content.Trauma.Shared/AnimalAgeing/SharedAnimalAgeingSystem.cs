// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chat;
using Content.Shared.Coordinates;
using Content.Shared.Damage.Components;
using Content.Shared.Examine;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Polymorph;
using Content.Shared.Polymorph.Systems;
using Content.Shared.Traits.Assorted;
using Content.Trauma.Shared.AnimalAgeing.Components;
using Content.Trauma.Shared.AnimalAgeing.Events;
using Content.Trauma.Shared.Ranching.Components;
using Content.Trauma.Shared.Ranching.Systems;
using Robust.Shared.Random;

namespace Content.Trauma.Shared.AnimalAgeing;

/// <summary>
/// This handles all the logic behind the animal ageing system
/// </summary>
public sealed partial class SharedAnimalAgeingSystem : EntitySystem
{
    [Dependency] private SharedSuicideSystem _suicide = default!;
    [Dependency] private HappinessSystem _happiness = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedPolymorphSystem _polymorph = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    public const string Polymorph = "ChickenRanchMorph";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnimalAgeingComponent, AddAgeToMobEvent>(OnAddAge);
        SubscribeLocalEvent<AnimalAgeingComponent, ChangeMobAgeStateEvent>(OnChangeState);
        SubscribeLocalEvent<AnimalAgeingComponent, OldAgeDeathEvent>(OnOldAgeDeath);

        SubscribeLocalEvent<AnimalAgeingComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<SpawnEntityOnAgeUpComponent, ChangeMobAgeStateEvent>(OnStateChangedAgeSpawn);

        SubscribeLocalEvent<SpawnEntityOnOldAgeDeathComponent, OldAgeDeathEvent>(OnOldAgeDeathSpawn);

        SubscribeLocalEvent<AgelessComponent, AddAgeToMobAttemptEvent>(OnAddAgeAttempt);
        SubscribeLocalEvent<AgelessComponent, ChangeMobAgeStateAttemptEvent>(OnChangeStateAttempt);
    }

    private void OnChangeStateAttempt(Entity<AgelessComponent> ent, ref ChangeMobAgeStateAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnAddAgeAttempt(Entity<AgelessComponent> ent, ref AddAgeToMobAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnOldAgeDeathSpawn(Entity<SpawnEntityOnOldAgeDeathComponent> ent, ref OldAgeDeathEvent args)
    {
        if (!TryComp<HappinessComponent>(ent.Owner,  out var happy))
            return;

        var happiness = _happiness.GetHappiness((ent.Owner, happy));

        if (happiness == null)
            return;

        EntProtoId entity = new();

        if (happiness <= ent.Comp.UnHappinessRequired)
            entity = ent.Comp.SadDeathEnt;

        if (happiness >= ent.Comp.HappinessRequired)
            entity = ent.Comp.HappyDeathEnt;

        SpawnAtPosition(entity, ent.Owner.ToCoordinates());
    }

    private void OnOldAgeDeath(Entity<AnimalAgeingComponent> ent, ref OldAgeDeathEvent args)
    {
        if (!TryComp<DamageableComponent>(ent.Owner,  out var damageable))
            return;

        _suicide.ApplyLethalDamage((ent.Owner, damageable), "Cellular");

        EnsureComp<UnrevivableComponent>(ent.Owner);
        RemComp<AnimalAgeingComponent>(ent.Owner);
    }

    private void OnStateChangedAgeSpawn(Entity<SpawnEntityOnAgeUpComponent> ent, ref ChangeMobAgeStateEvent args)
    {
        if (!TryComp<AnimalAgeingComponent>(ent.Owner, out var animalAgeing))
            return;

        if (animalAgeing.CurrentAgeState != ent.Comp.AgeToChangeAt)
            return;

        var enttospawn = _random.Pick(ent.Comp.EntToSpawn);

        CopyAndReplaceEntity(enttospawn, ent.Owner);
    }

    private void OnExamine(Entity<AnimalAgeingComponent> ent, ref ExaminedEvent args)
    {
        switch (ent.Comp.CurrentAgeState)
        {
            case AnimalAgeState.Baby:
                args.PushMarkup(Loc.GetString("age-markup-baby"));
                break;

            case AnimalAgeState.Adult:
                args.PushMarkup(Loc.GetString("age-markup-adult"));
                break;

            case AnimalAgeState.Senior:
                args.PushMarkup(Loc.GetString("age-markup-senior"));
                break;
        }
    }

    private void OnChangeState(Entity<AnimalAgeingComponent> ent, ref ChangeMobAgeStateEvent args)
    {
        ent.Comp.CurrentAgeState = args.NewState;
        Dirty(ent);
    }

    private void OnAddAge(Entity<AnimalAgeingComponent> ent, ref AddAgeToMobEvent args)
    {
        ent.Comp.YearsOld += args.Years;

        var yearsOld = ent.Comp.YearsOld;

        var attemptev = new ChangeMobAgeStateAttemptEvent();
        attemptev.Mob = ent;
        attemptev.NewState = ent.Comp.CurrentAgeState;

        if (yearsOld >= ent.Comp.AdultHoodYear && ent.Comp.CurrentAgeState == AnimalAgeState.Baby)
            attemptev.NewState = AnimalAgeState.Adult;

        if (yearsOld >= ent.Comp.SeniorHoodYear && ent.Comp.CurrentAgeState == AnimalAgeState.Adult)
            attemptev.NewState = AnimalAgeState.Senior;

        if (yearsOld >= ent.Comp.DeathYear && ent.Comp.CurrentAgeState == AnimalAgeState.Senior)
        {
            var attemptdeathev = new OldAgeDeathAttemptEvent(ent);
            RaiseLocalEvent(ent.Owner, ref attemptdeathev);

            if (attemptdeathev.Cancelled)
                return;

            var deathev = new OldAgeDeathEvent(ent);
            RaiseLocalEvent(ent.Owner, ref deathev);
        }

        Dirty(ent);
        if (attemptev.NewState != ent.Comp.CurrentAgeState)
        {
            RaiseLocalEvent(ent.Owner, ref attemptev);

            if (attemptev.Cancelled)
                return;

            var ev = new ChangeMobAgeStateEvent(ent, attemptev.NewState);
            RaiseLocalEvent(ent.Owner, ref ev);
        }
    }

    #region Helpers

    public void CopyAndReplaceEntity(EntProtoId entToSpawn, EntityUid uid)
    {
        if (!_proto.TryIndex<PolymorphPrototype>(Polymorph, out var proto))
            return;

        var poly = proto.Configuration;
        poly.Entity = entToSpawn;

        _polymorph.PolymorphEntity(uid, poly);
    }

    #endregion
}
