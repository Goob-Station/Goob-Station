using Content.Shared._Lavaland.Megafauna.Banana.Components;
using Robust.Shared.Network;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Systems;

public sealed class SpiralingSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SpiralingComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var spiral, out var transform))
        {
            if (!spiral.Initialized)
            {
                spiral.Origin = transform.LocalPosition;
                spiral.CurrentSpeed = spiral.SpiralSpeed;
                spiral.Radius = spiral.SpiralOutwards ? 0f : spiral.SpiralDistance;
                spiral.Initialized = true;
            }

            // Speed ramp to prevent Sonic from coming alive
            spiral.CurrentSpeed += spiral.SpiralAcceleration * frameTime;
            spiral.CurrentSpeed = MathF.Min(spiral.CurrentSpeed, spiral.SpiralMaxSpeed);

            spiral.Angle += spiral.CurrentSpeed * frameTime;

            if (spiral.SpiralOutwards)
            {
                spiral.Radius += spiral.CurrentSpeed * frameTime;
            }

            var offset = new Vector2(
                MathF.Cos(spiral.Angle),
                MathF.Sin(spiral.Angle)
            ) * spiral.Radius;

            transform.LocalPosition = spiral.Origin + offset;

            if (spiral.SpiralOutwards && spiral.Radius >= spiral.SpiralDistance)
            {
                if (_net.IsServer && spiral.DeleteOnEnd)
                {
                    QueueDel(uid);
                }
                else
                {
                    RemComp<SpiralingComponent>(uid);
                }
            }
        }
    }
}
