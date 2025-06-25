using Content.Server.Actions;
using Content.Shared._EE.Shadowling;
using Content.Shared.WhiteDream.BloodCult.Constructs.PhaseShift;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the Plane Shift ability.
/// A toogleable ability that lets you phase through walls!
/// </summary>
public sealed class ShadowlingPlaneShiftSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingPlaneShiftComponent, TogglePlaneShiftEvent>(OnPlaneShift);
    }

    private void OnPlaneShift(EntityUid uid, ShadowlingPlaneShiftComponent comp, TogglePlaneShiftEvent args)
    {
        comp.IsActive = !comp.IsActive;
        if (comp.IsActive)
        {
            TryDoShift(uid);
        }
        else
        {
            if (!HasComp<PhaseShiftedComponent>(uid))
                return;

            RemComp<PhaseShiftedComponent>(uid);
        }

        _actions.StartUseDelay(args.Action);
    }

    private void TryDoShift(EntityUid uid)
    {
        if (HasComp<PhaseShiftedComponent>(uid))
            return;

        var phaseShift = EnsureComp<PhaseShiftedComponent>(uid);
        phaseShift.MovementSpeedBuff = 1.7f;
        // Thanks to blood cult code for this component
    }
}
