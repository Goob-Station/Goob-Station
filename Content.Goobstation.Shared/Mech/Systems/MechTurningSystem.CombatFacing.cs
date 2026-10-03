using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Mech.Components;
using Content.Shared.Movement.Components;

namespace Content.Goobstation.Shared.Mech.Systems;

public sealed partial class MechTurningSystem
{
    private void InitializeCombatFacing()
    {
        SubscribeNetworkEvent<MechFaceRequestEvent>(OnFaceRequest);
    }

    private void OnFaceRequest(MechFaceRequestEvent msg, EntitySessionEventArgs args)
    {
        if (!TryGetEntity(msg.Mech, out var mechUid)
            || !TryComp<MechTurningComponent>(mechUid, out var turning))
            return;

        if (!TryComp<MechComponent>(mechUid, out var mechComp)
            || mechComp.Broken)
            return;

        if (args.SenderSession.AttachedEntity is not { } pilot
            || mechComp.PilotSlot.ContainedEntity != pilot)
            return;

        var target = GetReferenceRotation(mechUid.Value) + msg.Direction.ToAngle();
        RequestFacing((mechUid.Value, turning), pilot, target);
    }

    /// <summary>
    /// The frame cardinal facings are measured in. Matches WASD turning so it still works off-grid in space.
    /// </summary>
    public Angle GetReferenceRotation(EntityUid mech)
    {
        if (TryComp<InputMoverComponent>(mech, out var mover))
            return _mover.GetParentGridAngle(mover);

        return Transform(mech).GridUid is { } grid ? _xform.GetWorldRotation(grid) : Angle.Zero;
    }
}
