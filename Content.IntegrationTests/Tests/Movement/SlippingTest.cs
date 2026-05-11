#nullable enable
using System.Linq;
using Content.IntegrationTests.Tests.Helpers;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.Slippery;
using Content.Shared.StepTrigger.Components;
using Content.Shared.Stunnable;
using Robust.Shared.GameObjects;
using Robust.Shared.Input;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Components;

namespace Content.IntegrationTests.Tests.Movement;

public sealed class SlippingTest : MovementTest
{
    public sealed class SlipTestSystem : TestListenerSystem<SlipEvent>;

    [Test]
    public async Task BananaSlipTest()
    {
        await SpawnTarget("TrashBananaPeel");

        var modifier = Comp<MovementSpeedModifierComponent>(Player).SprintSpeedModifier;
        Assert.That(modifier, Is.EqualTo(1), "Player is not moving at full speed.");

        // Player is to the left of the banana peel.
        Assert.That(Delta(), Is.GreaterThan(0.5f));

        await LogSlipDebug("Initial");

        // Walking over the banana slowly does not trigger a slip.
        await SetKey(EngineKeyFunctions.Walk, BoundKeyState.Down);

        await Server.WaitPost(() =>
        {
            var mover = SEntMan.GetComponent<InputMoverComponent>(SPlayer);

            SLogger.Info(
                "[BananaSlipTest] Immediately after Walk Down\n" +
                "  HeldMoveButtons: {HeldMoveButtons}\n" +
                "  Has Walk: {HasWalk}\n" +
                "  Sprinting: {Sprinting}\n" +
                "  DefaultSprinting: {DefaultSprinting}",
                mover.HeldMoveButtons,
                (mover.HeldMoveButtons & MoveButtons.Walk) == MoveButtons.Walk,
                mover.Sprinting,
                mover.DefaultSprinting);
        });
        await RunTicks(1);
        await LogSlipDebug("After Walk Down");

        var banana = ToServer(Target!.Value);

        await Server.WaitPost(() =>
        {
            SEntMan.EnsureComponent<TestListenerComponent>(banana);
            ClearEvents<SlipEvent>(banana);
        });

        await LogSlipDebug("Before slow east move");
        await Move(DirectionFlag.East, 1f);
        await LogSlipDebug("After slow east move");

        var slipEvents = GetEvents<SlipEvent>(banana).ToList();

        SLogger.Info(
            "[BananaSlipTest] Slow move SlipEvent count: {Count}",
            slipEvents.Count);

        foreach (var slipEvent in slipEvents)
        {
            SLogger.Info(
                "[BananaSlipTest] Slow move SlipEvent\n" +
                "  Slipped: {Slipped}\n" +
                "  Expected player: {Player}\n" +
                "  Was player: {WasPlayer}",
                SEntMan.ToPrettyString(slipEvent.Slipped),
                SEntMan.ToPrettyString(SPlayer),
                slipEvent.Slipped == SPlayer);
        }

        Assert.That(slipEvents.Count, Is.EqualTo(0), "Walking over the banana slowly triggered a slip.");

        Assert.That(Delta(), Is.LessThan(0.5f));
        AssertComp<KnockedDownComponent>(false, Player);

        // Moving at normal speeds does trigger a slip.
        await SetKey(EngineKeyFunctions.Walk, BoundKeyState.Up);
        await RunTicks(1);
        await LogSlipDebug("After Walk Up");

        await AssertFiresEvent<SlipEvent>(async () =>
        {
            await LogSlipDebug("Before normal west move");
            await Move(DirectionFlag.West, 1f);
            await LogSlipDebug("After normal west move");
        });

        // And the person that slipped was the player
        AssertEvent<SlipEvent>(predicate: @event => @event.Slipped == SPlayer);
        AssertComp<KnockedDownComponent>(true, Player);
    }

    private async Task LogSlipDebug(string label)
    {
        await Server.WaitPost(() =>
        {
            var player = ToServer(Player);
            var target = ToServer(Target!.Value);

            var playerXform = SEntMan.GetComponent<TransformComponent>(player);
            var bananaXform = SEntMan.GetComponent<TransformComponent>(target);

            var playerPhysics = SEntMan.GetComponent<PhysicsComponent>(player);
            var playerMove = SEntMan.GetComponent<MovementSpeedModifierComponent>(player);

            SLogger.Info(
                "[BananaSlipTest] {Label}\n" +
                "  Player: {Player}\n" +
                "  Banana: {Banana}\n" +
                "  Player position: {PlayerPos}\n" +
                "  Banana position: {BananaPos}\n" +
                "  Delta: {Delta}\n" +
                "  Linear velocity: {Velocity}, length={VelocityLength}\n" +
                "  BodyStatus: {BodyStatus}\n" +
                "  SprintSpeedModifier: {SprintSpeedModifier}\n" +
                "  WalkSpeedModifier: {WalkSpeedModifier}\n" +
                "  KnockedDown: {KnockedDown}",
                label,
                SEntMan.ToPrettyString(player),
                SEntMan.ToPrettyString(target),
                playerXform.Coordinates,
                bananaXform.Coordinates,
                Delta(),
                playerPhysics.LinearVelocity,
                playerPhysics.LinearVelocity.Length(),
                playerPhysics.BodyStatus,
                playerMove.SprintSpeedModifier,
                playerMove.WalkSpeedModifier,
                SEntMan.HasComponent<KnockedDownComponent>(player));

            if (SEntMan.TryGetComponent<StepTriggerComponent>(target, out var step))
            {
                SLogger.Info(
                    "[BananaSlipTest] {Label} StepTrigger\n" +
                    "  Active: {Active}\n" +
                    "  StepOn: {StepOn}\n" +
                    "  RequiredTriggeredSpeed: {RequiredTriggeredSpeed}\n" +
                    "  IntersectRatio: {IntersectRatio}\n" +
                    "  Colliding: {Colliding}\n" +
                    "  CurrentlySteppedOn: {CurrentlySteppedOn}",
                    label,
                    step.Active,
                    step.StepOn,
                    step.RequiredTriggeredSpeed,
                    step.IntersectRatio,
                    string.Join(", ", step.Colliding.Select(uid => SEntMan.ToPrettyString(uid))),
                    string.Join(", ", step.CurrentlySteppedOn.Select(uid => SEntMan.ToPrettyString(uid))));
            }

            if (SEntMan.TryGetComponent<SlipperyComponent>(target, out var slippery))
            {
                SLogger.Info(
                    "[BananaSlipTest] {Label} Slippery\n" +
                    "  SlipOnStep: {SlipOnStep}\n" +
                    "  SuperSlippery: {SuperSlippery}\n" +
                    "  KnockdownTime: {KnockdownTime}\n" +
                    "  StunTime: {StunTime}\n" +
                    "  LaunchForwardsMultiplier: {LaunchForwardsMultiplier}\n" +
                    "  SlipFriction: {SlipFriction}\n" +
                    "  RequiredContact: checked via StepTrigger",
                    label,
                    slippery.SlipData.SlipOnStep,
                    slippery.SlipData.SuperSlippery,
                    slippery.SlipData.KnockdownTime,
                    slippery.SlipData.StunTime,
                    slippery.SlipData.LaunchForwardsMultiplier,
                    slippery.SlipData.SlipFriction);
            }

            if (SEntMan.TryGetComponent<InputMoverComponent>(player, out var mover))
            {
                SLogger.Info(
                    "[BananaSlipTest] {Label} InputMover\n" +
                    "  HeldMoveButtons: {HeldMoveButtons}\n" +
                    "  Has Walk: {HasWalk}\n" +
                    "  Has Right: {HasRight}\n" +
                    "  Has Left: {HasLeft}\n" +
                    "  Sprinting: {Sprinting}\n" +
                    "  DefaultSprinting: {DefaultSprinting}\n" +
                    "  CanMove: {CanMove}\n" +
                    "  LastInputTick: {LastInputTick}\n" +
                    "  LastInputSubTick: {LastInputSubTick}",
                    label,
                    mover.HeldMoveButtons,
                    (mover.HeldMoveButtons & MoveButtons.Walk) == MoveButtons.Walk,
                    (mover.HeldMoveButtons & MoveButtons.Right) == MoveButtons.Right,
                    (mover.HeldMoveButtons & MoveButtons.Left) == MoveButtons.Left,
                    mover.Sprinting,
                    mover.DefaultSprinting,
                    mover.CanMove,
                    mover.LastInputTick,
                    mover.LastInputSubTick);
            }
        });

        await RunTicks(1);
    }
}
