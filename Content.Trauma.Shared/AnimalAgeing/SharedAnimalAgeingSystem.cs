using Content.Goobstation.Shared.InternalResources.Components;
using Content.Shared.Chat;
using Content.Shared.Coordinates;
using Content.Shared.Damage.Components;
using Content.Shared.Examine;
using Content.Shared.Traits.Assorted;
using Content.Trauma.Shared.AnimalAgeing.Components;
using Content.Trauma.Shared.AnimalAgeing.Events;

namespace Content.Trauma.Shared.AnimalAgeing;

/// <summary>
/// This handles all the logic behind the animal ageing system
/// </summary>
public sealed class SharedAnimalAgeingSystem : EntitySystem
{
    [Dependency] private readonly SharedSuicideSystem _suicide = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AnimalAgeingComponent, AddAgeToMobEvent>(OnAddAge);
        SubscribeLocalEvent<AnimalAgeingComponent, ChangeMobAgeStateEvent>(OnChangeState);
        SubscribeLocalEvent<AnimalAgeingComponent, OldAgeDeathEvent>(OnOldAgeDeath);

        SubscribeLocalEvent<AnimalAgeingComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<ChangeComponentsOnAgeUpComponent, ChangeMobAgeStateEvent>(OnStateChanged);
        SubscribeLocalEvent<SpawnEntityOnAgeUpComponent, ChangeMobAgeStateEvent>(OnStateChangedAgeSpawn);
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

        var spawnedEnt = PredictedSpawnAtPosition(ent.Comp.EntToSpawn, ent.Owner.ToCoordinates());

        CopyComps(ent.Owner, spawnedEnt);

        // Directly copy age as CopyComps doesn't copy variables for some reason
        var addedAgeingComponent = CopyComp(ent.Owner,  spawnedEnt, animalAgeing);

        Dirty(spawnedEnt, addedAgeingComponent);
        PredictedDel(ent.Owner);
    }
    private void OnStateChanged(Entity<ChangeComponentsOnAgeUpComponent> ent, ref ChangeMobAgeStateEvent args)
    {
        if (args.NewState == AnimalAgeState.Adult)
        {
            EntityManager.AddComponents(ent.Owner, ent.Comp.AdultComponentsToAdd);
            EntityManager.RemoveComponents(ent.Owner, ent.Comp.AdultComponentsToRemove);
            return;
        }

        if (args.NewState == AnimalAgeState.Senior)
        {
            EntityManager.AddComponents(ent.Owner, ent.Comp.SeniorComponentsToAdd);
            EntityManager.RemoveComponents(ent.Owner, ent.Comp.SeniorComponentsToRemove);
            return;
        }
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
        RaiseLocalEvent(ent.Owner, ref attemptev);

        if (attemptev.Cancelled)
            return;

        var ev = new ChangeMobAgeStateEvent(ent, attemptev.NewState);
        RaiseLocalEvent(ent.Owner, ref ev);
    }
}
