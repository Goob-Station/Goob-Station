using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.LightDetection.Components;
using Content.Goobstation.Shared.LightDetection.Systems;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Threading;

namespace Content.Goobstation.Server.LightDetection;

/// <summary>
/// This system detects if an entity is standing on light.
/// It casts rays from the PointLight to the player.
/// </summary>
public sealed class LightDetectionSystem : SharedLightDetectionSystem
{
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    protected override string SawmillName => "light_damage";

    public float LookupRange = 10f;
    public float UpdateFrequency = 1f;

    private HandleLightJob _job;

    private float _accumulator = 1f;

    public override void Initialize()
    {
        base.Initialize();

        _job = new()
        {
            LightSys = this,
            XformSys = _transformSystem,
            PhysicsSys = _physicsSystem,
            LookupSys = _lookup,
        };

        Subs.CVar(_cfg, GoobCVars.LightDetectionRange, value => LookupRange = value, true);
        Subs.CVar(_cfg, GoobCVars.LightUpdateFrequency, value => UpdateFrequency = value, true);
    }

    public override void Update(float frameTime)
    {
        _accumulator -= frameTime;
        if (_accumulator > 0)
            return;

        _accumulator = UpdateFrequency;
        _job.UpdateEnts.Clear();

        var query = EntityQueryEnumerator<LightDetectionComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            _job.UpdateEnts.Add((uid, comp, xform));
        }

        _parallel.ProcessNow(_job, _job.UpdateEnts.Count);
    }

    private record struct HandleLightJob() : IParallelRobustJob
    {
        public int BatchSize => 16;

        public readonly List<Entity<LightDetectionComponent, TransformComponent>> UpdateEnts = new();

        public required LightDetectionSystem LightSys;
        public required SharedTransformSystem XformSys;
        public required SharedPhysicsSystem PhysicsSys;
        public required EntityLookupSystem LookupSys;

        public void Execute(int index)
        {
            var (uid, comp, xform) = UpdateEnts[index];

            var worldPos = XformSys.GetWorldPosition(xform);

            // We want to avoid this expensive operation if the user has not moved
            if ((comp.LastKnownPosition - worldPos).LengthSquared() < 0.01f)
                return;

            comp.LastKnownPosition = worldPos;
            comp.IsOnLight = false;
            var lookup = LookupSys.GetEntitiesInRange<PointLightComponent>(xform.Coordinates, LightSys.LookupRange);
            foreach (var ent in lookup)
            {
                var (point, pointLight) = ent;
                var pointXform = LightSys.Transform(point);

                if (!pointLight.Enabled)
                    continue;

                var lightPos = XformSys.GetWorldPosition(pointXform);
                var distance = (lightPos - worldPos).Length();

                if (distance <= 0.01f
                    || distance > pointLight.Radius)
                    continue;

                var direction = (worldPos - lightPos).Normalized();
                var ray = new CollisionRay(lightPos, direction, (int) CollisionGroup.Opaque);

                var rayResults = PhysicsSys.IntersectRay(
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
}
