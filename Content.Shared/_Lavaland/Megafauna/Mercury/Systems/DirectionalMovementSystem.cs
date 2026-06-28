using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Moves an entity towards the direction(s) set by bools, with optional acceleration.
/// </summary>
public sealed class DirectionalMovementSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DirectionalMovementComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var comp, out var transform))
        {
            var direction = Vector2.Zero;

            if (comp.MoveNorth)
            {
                direction += Vector2.UnitY;
            }

            if (comp.MoveSouth)
            {
                direction -= Vector2.UnitY;
            }

            if (comp.MoveEast)
            {
                direction += Vector2.UnitX;
            }

            if (comp.MoveWest)
            {
                direction -= Vector2.UnitX;
            }

            if (direction == Vector2.Zero)
            {
                comp.CurrentSpeed = 0f;
                continue;
            }

            direction = Vector2.Normalize(direction);

            float speed;
            if (comp.Acceleration > 0f)
            {
                comp.CurrentSpeed += comp.Acceleration * frameTime;
                comp.CurrentSpeed = MathF.Min(comp.CurrentSpeed, comp.Speed);
                speed = comp.CurrentSpeed;
            }
            else
            {
                speed = comp.Speed;
            }

            transform.LocalPosition += direction * speed * frameTime;
        }
    }
}
