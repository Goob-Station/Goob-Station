using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Moves an entity towards the specified direction set through a vector.
/// </summary>
public sealed class DirectionalMovementSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<DirectionalMovementComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var transform))
        {
            var deltaTime = (float) (curTime - comp.NextUpdate).TotalSeconds;
            comp.NextUpdate = curTime;

            if (comp.Direction == Vector2.Zero)
            {
                comp.CurrentSpeed = 0f;
                continue;
            }

            var direction = Vector2.Normalize(comp.Direction);

            float speed;
            if (comp.Acceleration > 0f)
            {
                comp.CurrentSpeed += comp.Acceleration * deltaTime;
                comp.CurrentSpeed = MathF.Min(comp.CurrentSpeed, comp.Speed);
                speed = comp.CurrentSpeed;
            }
            else
            {
                speed = comp.Speed;
            }

            transform.LocalPosition += direction * speed * deltaTime;
        }
    }
}
