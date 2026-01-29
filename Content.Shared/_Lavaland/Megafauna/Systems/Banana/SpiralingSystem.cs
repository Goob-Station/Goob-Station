using Content.Shared._Lavaland.Megafauna.Components.Banana;
using Content.Shared.Coordinates.Helpers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Systems.Banana;

public sealed class SpiralingSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    public override void Update(float frameTime)
    {
        foreach (var (spiral, transform) in EntityQuery<SpiralingComponent, TransformComponent>())
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

            // Radius only changes if spiraling outward
            if (spiral.SpiralOutwards)
                spiral.Radius += spiral.CurrentSpeed * frameTime;

            var offset = new Vector2(
                MathF.Cos(spiral.Angle),
                MathF.Sin(spiral.Angle)
            ) * spiral.Radius;

            transform.LocalPosition = spiral.Origin + offset;

            // End condition only applies to outward spiral
            if (spiral.SpiralOutwards &&
                spiral.Radius >= spiral.SpiralDistance)
            {
                if (_net.IsServer && spiral.DeleteOnEnd)
                {
                    QueueDel(transform.Owner);
                }
                else
                    RemComp<SpiralingComponent>(transform.Owner);
            }
        }
    }
}

