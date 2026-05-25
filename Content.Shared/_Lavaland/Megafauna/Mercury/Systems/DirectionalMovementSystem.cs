using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Moves an entity towards the specified direction set through a bool.
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

            if (comp.MoveNorth) direction += Vector2.UnitY;
            if (comp.MoveSouth) direction -= Vector2.UnitY;
            if (comp.MoveEast) direction += Vector2.UnitX;
            if (comp.MoveWest) direction -= Vector2.UnitX;

            if (direction == Vector2.Zero)
                continue;

            transform.LocalPosition += Vector2.Normalize(direction) * comp.Speed * frameTime;
        }
    }
}
