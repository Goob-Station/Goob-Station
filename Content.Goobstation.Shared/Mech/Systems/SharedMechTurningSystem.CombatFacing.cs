using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Mech.Components;

namespace Content.Goobstation.Shared.Mech.Systems;

public abstract partial class SharedMechTurningSystem
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

        if (Transform(mechUid.Value).GridUid is not { } grid)
            return;

        var target = Xform.GetWorldRotation(grid) + msg.Direction.ToAngle();
        RequestFacing((mechUid.Value, turning), pilot, target);
    }
}
