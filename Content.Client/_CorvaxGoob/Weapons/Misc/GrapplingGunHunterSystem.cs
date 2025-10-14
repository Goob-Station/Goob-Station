using Content.Shared._CorvaxGoob.Weapons.Misc;
using Content.Shared._CorvaxGoob.Weapons.Ranged.Components;
using Content.Client.Hands.Systems;
using Content.Shared.CombatMode;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Dynamics.Joints;

namespace Content.Client._CorvaxGoob.Weapons.Misc;

public sealed class GrapplingGunHunterSystem : SharedGrapplingGunHunterSystem
{
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly InputSystem _input = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!Timing.IsFirstTimePredicted)
            return;

        var local = _player.LocalEntity;
        var handUid = _hands.GetActiveHandEntity();

        if (!TryComp<GrapplingGunHunterComponent>(handUid, out var grappling))
            return;

        if (!TryComp<JointComponent>(handUid, out var jointComp) ||
            !jointComp.GetJoints.TryGetValue(GrapplingJoint, out var joint) ||
            joint is not DistanceJoint distance)
        {
            return;
        }

        if (distance.MaxLength <= distance.MinLength)
            return;

        var reelKey = _input.CmdStates.GetState(EngineKeyFunctions.UseSecondary) == BoundKeyState.Down;

        if (!TryComp<CombatModeComponent>(local, out var combatMode) ||
            !combatMode.IsInCombatMode)
        {
            reelKey = false;
        }

        if (grappling.Reeling == reelKey)
            return;

        RaisePredictiveEvent(new RequestGrapplingHunterReelMessage(reelKey));
    }
}
