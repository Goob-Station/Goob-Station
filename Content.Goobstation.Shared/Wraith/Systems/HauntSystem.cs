using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Shared.Interaction;
using Robust.Shared.Timing;
using Content.Shared.Actions;
using Content.Shared.Flash.Components;
using Content.Shared.Revenant.Components;
using Content.Shared.StatusEffect;

namespace Content.Goobstation.Shared.Wraith.Systems;
//Partially ported from Impstation
public sealed partial class HauntSystem : EntitySystem
{
    [Dependency] private readonly SharedInteractionSystem _interact = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsOld = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly WraithPointsSystem _wraithPointsSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    private EntityQuery<HauntedComponent> _hauntQuery;
    private EntityQuery<WraithAbsorbableComponent> _wraithAbsorbableQuery;
    public override void Initialize()
    {
        base.Initialize();

        _hauntQuery = GetEntityQuery<HauntedComponent>();
        _wraithAbsorbableQuery = GetEntityQuery<WraithAbsorbableComponent>();

        SubscribeLocalEvent<HauntComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<HauntComponent, ComponentShutdown>(OnComponentShutdown);

        SubscribeLocalEvent<HauntComponent, HauntEvent>(OnHaunt);
    }
    //TO DO: Add action to stop corporeal form.
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HauntComponent>();
        while (query.MoveNext(out var uid, out var haunt))
        {
            if (_timing.CurTime >= haunt.NextHauntWpRegenUpdate && haunt.WpBoostActive)
            {
                // reset
                _wraithPointsSystem.SetWpRate(haunt.OriginalWpRegen, uid);
                haunt.WpBoostActive = false;
                Dirty(uid, haunt);
            }

            if (!haunt.Active)
                continue;

            if (_timing.CurTime >= haunt.NextHauntUpdate)
            {
                RemComp<CorporealComponent>(uid);
                haunt.Active = false;
                _actions.StartUseDelay(haunt.ActionEnt);

                Dirty(uid, haunt);
            }

            // constantly check for witnesses
            if (_timing.CurTime >= haunt.WitnessNextUpdate)
            {
                var lookup = _lookup.GetEntitiesInRange(uid, 10f); // 10f should cover your view-range
                foreach (var entity in lookup)
                {
                    // skip if we are already haunted, or if we cant be haunted
                    if (_hauntQuery.HasComp(entity) || !_wraithAbsorbableQuery.HasComp(entity))
                        continue;

                    if (!_interact.InRangeUnobstructed(uid, entity, 10f))
                        continue;

                    EnsureComp<HauntedComponent>(entity);
                    _wraithPointsSystem.AdjustWpGenerationRate(haunt.HauntWpRegenPerWitness, uid);
                }

                haunt.WitnessNextUpdate = _timing.CurTime + haunt.WitnessUpdate;
                Dirty(uid, haunt);
            }
        }
    }

    private void OnHaunt(Entity<HauntComponent> ent, ref HauntEvent args)
    {
        if (ent.Comp.Active)
        {
            _statusEffectsOld.TryRemoveStatusEffect(ent.Owner, ent.Comp.CorporealEffect);
            _wraithPointsSystem.SetWpRate(ent.Comp.OriginalWpRegen, ent.Owner);
            ent.Comp.Active = false;
            ent.Comp.WpBoostActive = false;
            args.Handled = true;
            Dirty(ent);

            return;
        }

        // flash people nearby
        var lookup = _lookup.GetEntitiesInRange(ent.Owner, 3f);
        foreach (var entity in lookup)
            _statusEffectsOld.TryAddStatusEffect<FlashedComponent>(entity,
                ent.Comp.FlashedId,
                ent.Comp.HauntFlashDuration,
                true);

        // we don't have corporeal so add it
        _statusEffectsOld.TryAddStatusEffect<CorporealComponent>(ent.Owner, ent.Comp.CorporealEffect, ent.Comp.HauntCorporealDuration, true);

        // set original rate for resetting it after boost
        ent.Comp.OriginalWpRegen = _wraithPointsSystem.GetCurrentWpRate(ent.Owner);

        // activate the haunt timer and start tracking people
        ent.Comp.Active = true;
        ent.Comp.NextHauntUpdate = _timing.CurTime + ent.Comp.HauntDuration;
        ent.Comp.WitnessNextUpdate = _timing.CurTime + ent.Comp.WitnessUpdate;

        // boost wp regen per witness
        ent.Comp.HauntWpRegenDuration = _timing.CurTime + ent.Comp.HauntWpRegenDuration;
        ent.Comp.WpBoostActive = true;
        Dirty(ent);
    }

    private void OnMapInit(Entity<HauntComponent> ent, ref MapInitEvent args) =>
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnComponentShutdown(Entity<HauntComponent> ent, ref ComponentShutdown args) =>
        _actions.RemoveAction(ent.Comp.ActionEnt);
}
