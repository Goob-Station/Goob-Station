using System.Numerics;
using Content.Shared.Friction;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Nullspace;

public sealed partial class NullspaceMoverController : VirtualController
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedNullspaceSystem _nullspace = default!;

    public override void Initialize()
    {
        UpdatesAfter.Add(typeof(TileFrictionController));
        base.Initialize();
    }

    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        var query = EntityQueryEnumerator<NullspacePlayerComponent, InputMoverComponent, PhysicsComponent>();
        while (query.MoveNext(out var uid, out var player, out var mover, out var body))
        {
            if (prediction && !body.Predict)
                continue;

            Move((uid, player), mover, body, frameTime);
        }
    }

    private void Move(Entity<NullspacePlayerComponent> ent, InputMoverComponent mover, PhysicsComponent body, float frameTime)
    {
        var player = ent.Comp;
        var started = _nullspace.TryGetMap(ent, out var map) && map.Comp.StartTime != null;

        var buttons = mover.HeldMoveButtons;
        var forward = 0;
        var side = 0;
        if (started && !player.Frozen)
        {
            if ((buttons & MoveButtons.Up) != 0)
                forward++;
            if ((buttons & MoveButtons.Down) != 0)
                forward--;
            if ((buttons & MoveButtons.Right) != 0)
                side++;
            if ((buttons & MoveButtons.Left) != 0)
                side--;
        }

        var moving = forward != 0 || side != 0;
        var wantSprint = (buttons & MoveButtons.Walk) != 0 && moving && forward >= 0;

        var stamina = player.Stamina;
        var exhausted = player.Exhausted;
        if (exhausted && stamina >= NullspaceParams.ExhaustedUntil)
            exhausted = false;

        var sprinting = wantSprint && !exhausted && stamina > 0f;
        if (sprinting)
        {
            stamina = MathF.Max(0f, stamina - NullspaceParams.StaminaDrain * frameTime);
            if (stamina <= 0f)
                exhausted = true;
        }
        else if (started)
        {
            var regen = moving ? NullspaceParams.StaminaRegenMoving : NullspaceParams.StaminaRegenStill;
            stamina = MathF.Min(1f, stamina + regen * frameTime);
        }

        if (stamina != player.Stamina || exhausted != player.Exhausted || sprinting != player.Sprinting)
        {
            player.Stamina = stamina;
            player.Exhausted = exhausted;
            player.Sprinting = sprinting;
            Dirty(ent);
        }

        var yaw = Angle.Lerp(player.PrevYaw, player.Yaw, 0.5f);
        if (_timing.IsFirstTimePredicted)
            player.PrevYaw = player.Yaw;

        var wish = Vector2.Zero;
        if (moving)
        {
            var speed = sprinting
                ? NullspaceParams.Sprint
                : exhausted ? NullspaceParams.ExhaustedWalk : NullspaceParams.Walk;
            var look = yaw.RotateVec(Vector2.UnitY);
            var right = new Vector2(look.Y, -look.X);
            wish = Vector2.Normalize(look * forward + right * side) * (speed / NullspaceParams.Cell);
        }

        var velocity = body.LinearVelocity;
        velocity += (wish - velocity) * (1f - MathF.Exp(-NullspaceParams.MoveDamp * frameTime));
        if (velocity.LengthSquared() < 1e-6f)
            velocity = Vector2.Zero;

        PhysicsSystem.SetLinearVelocity(ent, velocity, body: body);
    }
}
