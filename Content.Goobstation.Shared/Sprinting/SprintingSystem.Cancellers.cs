using Content.Shared.Buckle.Components;
using Content.Shared.Cuffs.Components;
using Content.Shared.Gravity;
using Content.Shared.Mech.Components;
using Content.Shared.Standing;

namespace Content.Goobstation.Shared.Sprinting;

public sealed partial class SprintingSystem
{
    /// <summary>
    /// Returns whether an entity is able to sprint.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="failLoc"></param>
    /// <returns></returns>
    public bool CanSprint(Entity<SprinterComponent?> ent, out LocId failLoc)
    {
        failLoc = "sprinting-cant-generic";

        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!ent.Comp.CanSprint)
            return false;

        var ev = new SprintAttemptEvent();
        RaiseLocalEvent(ent, ref ev);

        failLoc = ev.FailLoc ?? failLoc;
        return !ev.Cancelled;
    }

    [SubscribeLocalEvent]
    private void OnGravityAffectedSprintAttempt(Entity<GravityAffectedComponent> ent, ref SprintAttemptEvent args)
    {
        if (!_gravity.IsWeightless(ent.Owner))
            return;

        args.FailLoc = "sprinting-cant-weightles";
        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnCuffableSprintAttempt(Entity<CuffableComponent> ent, ref SprintAttemptEvent args)
    {
        if (ent.Comp.CanStillInteract)
            return;

        args.FailLoc = "sprinting-cant-restrained";
        args.Cancel();
    }

    private void OnStandingStateSprintAttempt(Entity<StandingStateComponent> ent, ref SprintAttemptEvent args)
    {
        if (!_standing.IsDown(ent.Owner))
            return;

        args.FailLoc = "sprinting-cant-downed";
        args.Cancel();
    }

    private void OnBuckleSprintAttempt(Entity<BuckleComponent> ent, ref SprintAttemptEvent args)
    {
        if (ent.Comp.BuckledTo == null
            || !TryComp<SprinterComponent>(ent.Comp.BuckledTo, out var sprinterComponent)
            || sprinterComponent.IsSprinting)
            return;

        args.FailLoc = "sprinting-cant-buckled";
        args.Cancel();
    }

    private void OnMechPilotSprintAttempt(Entity<MechPilotComponent> ent, ref SprintAttemptEvent args)
    {
        if (!TryComp<SprinterComponent>(ent.Comp.Mech, out var sprinterComponent)
            || sprinterComponent.IsSprinting)
            return;

        args.FailLoc = "sprinting-cant-mech";
        args.Cancel();
    }
}