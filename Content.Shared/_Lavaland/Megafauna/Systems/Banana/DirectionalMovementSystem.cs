using Content.Shared._Lavaland.Megafauna.Components.Banana;
using Robust.Shared.Network;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Systems.Banana;

public sealed class DirectionalMovementSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    public override void Update(float frameTime)
    {
        foreach (var (move, transform) in EntityQuery<DirectionalMovementComponent, TransformComponent>())
        {
            if (!move.Initialized)
            {
                move.Origin = transform.LocalPosition;
                move.CurrentSpeed = move.InitialSpeed;

                if (move.Direction != Vector2.Zero)
                    move.Direction = Vector2.Normalize(move.Direction);

                move.Initialized = true;
            }

            // Accelerate
            move.CurrentSpeed += move.Acceleration * frameTime;
            move.CurrentSpeed = MathF.Min(move.CurrentSpeed, move.MaxSpeed);

            // Move entity
            var delta = move.Direction * move.CurrentSpeed * frameTime;
            transform.LocalPosition += delta;

            // Track distance
            move.DistanceTraveled += delta.Length();

            // End condition
            if (move.MaxDistance > 0f &&
                move.DistanceTraveled >= move.MaxDistance)
            {
                if (_net.IsServer && move.DeleteOnEnd)
                {
                    QueueDel(transform.Owner);
                }
                else
                {
                    RemComp<DirectionalMovementComponent>(transform.Owner);
                }
            }
        }
    }
}
