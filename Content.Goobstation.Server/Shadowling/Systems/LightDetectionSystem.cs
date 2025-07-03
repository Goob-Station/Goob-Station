using Content.Goobstation.Shared.Shadowling.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;

namespace Content.Goobstation.Server.Shadowling.Systems;

/// <summary>
/// This system detects if an entity is standing on light.
/// It casts rays from the PointLight to the player.
/// </summary>
public sealed class LightDetectionSystem : EntitySystem
{
    [Dependency] private readonly PhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    protected override string SawmillName => "light_damage";

    [DataField] public float LookupRange = 10f;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<LightDetectionComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            comp.Accumulator -= frameTime;
            if (comp.Accumulator > 0)
                continue;

            comp.Accumulator = comp.UpdateInterval;

            if (_mobStateSystem.IsDead(uid))
                continue;

            DetectLight(uid, comp, xform);
        }
    }

    private void DetectLight(EntityUid uid, LightDetectionComponent comp, TransformComponent xform)
    {
        var worldPos = _transformSystem.GetWorldPosition(xform);

        // We want to avoid this expensive operation if the user has not moved
        if ((comp.LastKnownPosition - worldPos).LengthSquared() < 0.01f)
            return;

        comp.LastKnownPosition = worldPos;
        comp.IsOnLight = false;
        var lookup = _lookup.GetEntitiesInRange<PointLightComponent>(xform.Coordinates, LookupRange);
        foreach (var ent in lookup)
        {
            var (point, pointLight) = ent;
            var pointXform = Transform(point);

            if (!pointLight.Enabled)
                continue;

            var lightPos = _transformSystem.GetWorldPosition(pointXform);
            var distance = (lightPos - worldPos).Length();

            if (distance <= 0.01f
                || distance > pointLight.Radius)
                continue;

            var direction = (worldPos - lightPos).Normalized();
            var ray = new CollisionRay(lightPos, direction, (int)CollisionGroup.Opaque);

            var rayResults = _physicsSystem.IntersectRay(
                xform.MapID,
                ray,
                distance,
                point);

            var hasBeenBlocked = false;
            foreach (var result in rayResults)
            {
                if (result.HitEntity != uid)
                {
                    hasBeenBlocked = true;
                    break;
                }
            }

            if (hasBeenBlocked)
                continue;

            comp.IsOnLight = true;
            return;
        }
    }
}
