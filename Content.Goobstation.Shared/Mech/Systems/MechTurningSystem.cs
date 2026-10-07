using System.Numerics;
using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.CombatMode;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Mech.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Mech.Systems;

public sealed partial class MechTurningSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedTransformSystem _xform = default!;
    [Dependency] private SharedMoverController _mover = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechTurningComponent, MoveInputEvent>(OnMechMoveInput);
        SubscribeLocalEvent<MechPilotComponent, MoveInputEvent>(OnPilotMoveInput);
        SubscribeLocalEvent<MechTurningComponent, ChangeDirectionAttemptEvent>(OnChangeDirectionAttempt);
        SubscribeLocalEvent<MechTurningComponent, UpdateCanMoveEvent>(OnCanMove);
        SubscribeLocalEvent<MechTurningComponent, MechTurnDoAfterEvent>(OnTurnFinished);

        InitializeCombatFacing();
    }

    private void OnChangeDirectionAttempt(EntityUid uid, MechTurningComponent mech, ChangeDirectionAttemptEvent args) =>
        args.Cancel();

    private void OnMechMoveInput(Entity<MechTurningComponent> mech, ref MoveInputEvent args) =>
        HandleMoveInput(mech);

    private void OnPilotMoveInput(Entity<MechPilotComponent> pilot, ref MoveInputEvent args)
    {
        if (TryComp<MechTurningComponent>(pilot.Comp.Mech, out var turning))
            HandleMoveInput((pilot.Comp.Mech, turning));
    }

    private void HandleMoveInput(Entity<MechTurningComponent> mech)
    {
        if (_net.IsClient)
            return;

        if (!TryComp<MechComponent>(mech, out var mechComp) || mechComp.PilotSlot.ContainedEntity is not { } pilot)
            return;

        if (mechComp.Broken || !TryComp<InputMoverComponent>(mech, out var mover))
            return;

        if (TryComp<CombatModeComponent>(pilot, out var combat) && combat.IsInCombatMode)
            return;

        var buttons = mover.HeldMoveButtons;
        var x = (buttons.HasFlag(MoveButtons.Right) ? 1f : 0f) - (buttons.HasFlag(MoveButtons.Left) ? 1f : 0f);
        var y = (buttons.HasFlag(MoveButtons.Up) ? 1f : 0f) - (buttons.HasFlag(MoveButtons.Down) ? 1f : 0f);
        var dirVec = new Vector2(x, y);
        if (dirVec.LengthSquared() < 0.01f)
            return;

        if (x != 0f && y != 0f)
            return;

        var wish = Vector2.Normalize(_mover.GetParentGridAngle(mover).RotateVec(dirVec));
        RequestFacing(mech, pilot, wish.ToWorldAngle());
    }

    private void RequestFacing(Entity<MechTurningComponent> mech, EntityUid pilot, Angle target)
    {
        var offBy = Math.Abs(Angle.ShortestDistance(_xform.GetWorldRotation(mech), target).Degrees);

        if (offBy < mech.Comp.SnapArcDegrees)
        {
            if (mech.Comp.IsTurning)
                CancelTurn(mech);
            else
                _xform.SetWorldRotation(mech.Owner, target);
            return;
        }

        if (mech.Comp.IsTurning && Math.Abs(Angle.ShortestDistance(mech.Comp.TargetRotation, target).Degrees) < 1f)
            return;

        StartTurn(mech, pilot, target, offBy);
    }

    private void StartTurn(Entity<MechTurningComponent> mech, EntityUid pilot, Angle target, double degrees)
    {
        if (mech.Comp.DoAfter != null)
            _doAfter.Cancel(mech.Comp.DoAfter);

        var delay = degrees > 135 ? mech.Comp.FullTurnDelay : mech.Comp.ShortTurnDelay;

        mech.Comp.TargetRotation = target;
        mech.Comp.IsTurning = true;
        Dirty(mech);
        _blocker.UpdateCanMove(mech);

        var doAfter = new DoAfterArgs(EntityManager, pilot, delay, new MechTurnDoAfterEvent(), mech, target: mech)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            BreakOnHandChange = false,
            NeedHand = false,
            RequireCanInteract = false,
            MultiplyDelay = false,
            Hidden = true,
        };

        if (_doAfter.TryStartDoAfter(doAfter, out var id))
        {
            mech.Comp.DoAfter = id;
            return;
        }

        mech.Comp.IsTurning = false;
        Dirty(mech);
        _blocker.UpdateCanMove(mech);
    }

    private void CancelTurn(Entity<MechTurningComponent> mech)
    {
        var id = mech.Comp.DoAfter;
        mech.Comp.DoAfter = null;
        mech.Comp.IsTurning = false;
        Dirty(mech);
        _blocker.UpdateCanMove(mech);

        if (id != null)
            _doAfter.Cancel(id);
    }

    private void OnCanMove(Entity<MechTurningComponent> mech, ref UpdateCanMoveEvent args)
    {
        if (mech.Comp.IsTurning)
            args.Cancel();
    }

    private void OnTurnFinished(Entity<MechTurningComponent> mech, ref MechTurnDoAfterEvent args)
    {
        if (args.Handled || _net.IsClient)
            return;

        args.Handled = true;
        mech.Comp.DoAfter = null;
        mech.Comp.IsTurning = false;

        if (!args.Cancelled)
            _xform.SetWorldRotation(mech.Owner, mech.Comp.TargetRotation);

        Dirty(mech);
        _blocker.UpdateCanMove(mech);
    }
}
