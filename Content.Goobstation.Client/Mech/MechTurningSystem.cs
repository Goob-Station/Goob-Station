using Content.Goobstation.Shared.Mech;
using Content.Goobstation.Shared.Mech.Components;
using Content.Goobstation.Shared.Mech.Systems;
using Content.Shared.CombatMode;
using Content.Shared.Mech.Components;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Mech;

public sealed class MechTurningSystem : SharedMechTurningSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_player.LocalEntity is not { } player
            || !TryComp<MechPilotComponent>(player, out var pilot)
            || !TryComp<MechTurningComponent>(pilot.Mech, out var turning))
            return;

        var mech = pilot.Mech;
        var realTime = _timing.RealTime;
        if (realTime < turning.NextPoll)
            return;

        turning.NextPoll = realTime + turning.PollInterval;

        if (!TryComp<CombatModeComponent>(player, out var combat) || !combat.IsInCombatMode)
        {
            turning.LastRequested = null;
            return;
        }

        var xform = Transform(mech);
        if (xform.GridUid is not { } grid)
            return;

        var mouse = _eye.PixelToMap(_input.MouseScreenPosition);
        if (mouse.MapId != xform.MapID)
            return;

        var (mechPos, mechRot) = Xform.GetWorldPositionRotation(xform);
        var delta = mouse.Position - mechPos;
        if (delta.LengthSquared() < 0.25f)
            return;
        var gridRot = Xform.GetWorldRotation(grid);
        var wanted = (-gridRot).RotateVec(delta).ToWorldAngle().GetCardinalDir();
        var current = (mechRot - gridRot).GetCardinalDir();

        if (turning.IsTurning)
        {
            var turningTo = (turning.TargetRotation - gridRot).GetCardinalDir();
            if (wanted == turningTo)
                return;
        }
        else if (wanted == current)
        {
            turning.LastRequested = null;
            return;
        }

        if (turning.LastRequested == wanted && realTime - turning.LastSend < turning.ResendInterval)
            return;

        turning.LastRequested = wanted;
        turning.LastSend = realTime;
        RaiseNetworkEvent(new MechFaceRequestEvent(GetNetEntity(mech), wanted));
    }
}
