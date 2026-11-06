using Content.Goobstation.Common.Grab;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Actions;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Slasher.Systems;

public sealed partial class SlasherRelentlessGrabSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedDoorSystem _door = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SlasherFearSystem _fear = default!;

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<SlasherRelentlessGrabComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);
    }

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<SlasherRelentlessGrabComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEnt);
    }

    [SubscribeLocalEvent]
    private void OnGrab(Entity<SlasherRelentlessGrabComponent> ent, ref SlasherRelentlessGrabEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (TryComp<DoorComponent>(target, out var door))
        {
            args.Handled = TryJackDoor(ent, (target, door));
            return;
        }

        var fear = _fear.GetFear(ent.Owner);
        var stage = fear >= ent.Comp.HardGrabFear ? GrabStage.Hard : GrabStage.Soft;

        if (!_pulling.CanPull(ent.Owner, target)
            || !_pulling.TryStartPull(ent.Owner, target, grabStageOverride: stage, force: true))
        {
            _popup.PopupClient(Loc.GetString("slasher-relentless-grab-fail"), target, ent.Owner);
            return;
        }

        if (fear >= ent.Comp.StunGrabFear)
            _stun.TryUpdateStunDuration(target, ent.Comp.GrabStunDuration);

        args.Handled = true;
    }

    private bool TryJackDoor(Entity<SlasherRelentlessGrabComponent> ent, Entity<DoorComponent> door)
    {
        if (door.Comp.State == DoorState.Welded)
        {
            _popup.PopupClient(Loc.GetString("slasher-relentless-grab-door-welded"), door.Owner, ent.Owner);
            return false;
        }

        var jacked = EnsureComp<SlasherJackedDoorComponent>(door.Owner);
        jacked.ExpiresAt = _timing.CurTime + ent.Comp.DoorDisableDuration;
        Dirty(door.Owner, jacked);

        if (door.Comp.State is DoorState.Closed or DoorState.Closing or DoorState.Denying or DoorState.Emagging)
            _door.StartOpening(door.Owner, door.Comp, ent.Owner, predicted: true);

        if (ent.Comp.DoorJackEffect is { } effect)
            PredictedSpawnAtPosition(effect, Transform(door.Owner).Coordinates);

        return true;
    }
}
