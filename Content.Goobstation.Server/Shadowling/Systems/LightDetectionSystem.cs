using Content.Shared._EE.Shadowling.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Timing;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This system detects if an entity is standing on light.
/// It casts rays from the PointLight to the player.
/// </summary>
public sealed class LightDetectionSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly PhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightDetectionComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(EntityUid uid, LightDetectionComponent component, ComponentStartup args)
    {
        component.NextUpdate = _timing.CurTime;
    }
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<LightDetectionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // Skip dead entities
            if (_mobStateSystem.IsDead(uid))
                continue;

            if (_timing.CurTime < comp.NextUpdate)
                continue;

            comp.NextUpdate += comp.UpdateInterval;
            DetectLight(uid, comp);
        }
    }

    private void DetectLight(EntityUid uid, LightDetectionComponent comp)
    {
        var xform = EntityManager.GetComponent<TransformComponent>(uid);
        var worldPos = _transformSystem.GetWorldPosition(uid);

        // We want to avoid this expensive operation if the user has not moved
        if ((comp.LastKnownPosition - worldPos).LengthSquared() < 0.01f)
            return;

        comp.LastKnownPosition = worldPos;
        comp.IsOnLight = false;
        var query = EntityQueryEnumerator<PointLightComponent>();
        while (query.MoveNext(out var point, out var pointLight))
        {
            if (!pointLight.Enabled)
                continue;

            var lightPos = _transformSystem.GetWorldPosition(point);
            var distance = (lightPos - worldPos).Length();

            if (distance <= 0.01f) // So the debug stops crashing
                continue;

            if (distance > pointLight.Radius)
                continue;

            var direction = (worldPos - lightPos).Normalized();
            var ray = new CollisionRay(lightPos, direction, (int)CollisionGroup.Opaque);

            var rayResults = _physicsSystem.IntersectRay(
                xform.MapID,
                ray,
                distance,
                point); // todo: remove this once slings get night vision action


            var hasBeenBlocked = false;
            foreach (var result in rayResults)
            {
                if (result.HitEntity != uid)
                {
                    hasBeenBlocked = true;
                    break;
                }
            }

            if (!hasBeenBlocked)
            {
                comp.IsOnLight = true;
                return;
            }
        }

    }
}
