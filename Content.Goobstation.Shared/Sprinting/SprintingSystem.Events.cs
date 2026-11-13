using Content.Goobstation.Common.Movement;
using Content.Shared._EinsteinEngines.Flight.Events;
using Content.Shared.Bed.Sleep;
using Content.Shared.CombatMode;
using Content.Shared.Mech.EntitySystems;
using Content.Shared.Mobs;
using Content.Shared.Standing;
using Content.Shared.Stunnable;

namespace Content.Goobstation.Shared.Sprinting;

public sealed partial class SprintingSystem
{
    [SubscribeLocalEvent]
    private void OnMobStateChangedEvent(Entity<SprinterComponent> ent, ref MobStateChangedEvent args)
    {
        if (!ent.Comp.IsSprinting
            || args.NewMobState is MobState.Alive)
            return;

        SetSprinting(ent.AsNullable(), false, gracefulStop: false);
    }

    [SubscribeLocalEvent]
    private void OnSleep(Entity<SprinterComponent> ent, ref SleepStateChangedEvent args)
    {
        if (!ent.Comp.IsSprinting
            || !args.FellAsleep)
            return;

        SetSprinting(ent.AsNullable(), false, gracefulStop: false);
    }

    [SubscribeLocalEvent]
    private void OnKnockedDown(Entity<SprinterComponent> ent, ref KnockedDownEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        SetSprinting(ent.AsNullable(), false, gracefulStop: false);
    }

    [SubscribeLocalEvent]
    private void OnStunned(Entity<SprinterComponent> ent, ref StunnedEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        SetSprinting(ent.AsNullable(), false, gracefulStop: false);
    }

    [SubscribeLocalEvent]
    private void OnDowned(Entity<SprinterComponent> ent, ref DownedEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        SetSprinting(ent.AsNullable(), false, gracefulStop: false);
    }

    [SubscribeLocalEvent]
    private void OnFlight(Entity<SprinterComponent> ent, ref FlightEvent args)
    {
        if (!ent.Comp.IsSprinting
            || args.IsFlying)
            return;

        SetSprinting(ent.AsNullable(), false);
    }

    [SubscribeLocalEvent]
    private void OnMechEntry(Entity<SprinterComponent> ent, ref MechEntryEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        SetSprinting(ent.AsNullable(), false);
    }

    [SubscribeLocalEvent]
    private void OnToggleWalk(Entity<SprinterComponent> ent, ref ToggleWalkEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        SetSprinting(ent.AsNullable(), false);
    }

    [SubscribeLocalEvent]
    private void OnDisarm(Entity<SprinterComponent> ent, ref DisarmedEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        _staminaSystem.TakeStaminaDamage(ent, ent.Comp.StaminaPenaltyOnShove, logDamage: false);
        SetSprinting(ent.AsNullable(), false);
    }
}
