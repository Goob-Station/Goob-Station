using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Systems.Banana;

/// <summary>
/// This system handles orbiting an entity around its owner.
/// </summary>
public sealed class OrbitingSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<OrbitingComponent>();
        while (query.MoveNext(out var uid, out var orbit))
        {
            if (!Exists(orbit.Owner))
            {
                QueueDel(uid);
                continue;
            }

            orbit.Radius = MathF.Min(
                orbit.Radius + orbit.GrowSpeed * frameTime,
                orbit.MaxRadius);

            orbit.Angle += orbit.RotationSpeed * frameTime;

            var ownerPos = _xform.GetWorldPosition(orbit.Owner);
            var offset = new Vector2(
                MathF.Cos(orbit.Angle),
                MathF.Sin(orbit.Angle)) * orbit.Radius;

            _xform.SetWorldPosition(uid, ownerPos + offset);
        }
    }
}

