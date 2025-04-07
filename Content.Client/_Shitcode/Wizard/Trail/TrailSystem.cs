using System.Linq;
using System.Numerics;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Goobstation.Wizard.TimeStop;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Animations;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._Shitcode.Wizard.Trail;

public sealed class TrailSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private EntityQuery<TransformComponent> _xformQuery;
    private EntityQuery<FrozenComponent> _frozenQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new TrailOverlay(EntityManager, _protoMan, _timing));

        SubscribeLocalEvent<TrailComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<TrailComponent, ComponentStartup>(OnStartup);

        _xformQuery = GetEntityQuery<TransformComponent>();
        _frozenQuery = GetEntityQuery<FrozenComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        UpdatesOutsidePrediction = true;
    }

    private void OnStartup(Entity<TrailComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Accumulator = ent.Comp.Frequency;
        ent.Comp.LerpAccumulator = ent.Comp.LerpTime;
    }

    private void OnRemove(Entity<TrailComponent> ent, ref ComponentRemove args)
    {
        var (_, comp) = ent;

        if (!comp.SpawnRemainingTrail || comp.TrailData.Count == 0 || comp.Frequency <= 0f || comp.Lifetime <= 0f)
            return;

        if (comp.LastCoords.MapId != _eye.CurrentEye.Position.MapId)
            return;

        if (comp.RenderedEntity != null && TerminatingOrDeleted(comp.RenderedEntity.Value))
            return;

        var remainingTrail = Spawn(null, comp.LastCoords);
        EnsureComp<TimedDespawnComponent>(remainingTrail).Lifetime = comp.Lifetime;
        var trail = EnsureComp<TrailComponent>(remainingTrail);
        trail.SpawnRemainingTrail = false;
        trail.Frequency = 0f;
        trail.Lifetime = comp.Lifetime;
        trail.AlphaLerpAmount = comp.AlphaLerpAmount;
        trail.ScaleLerpAmount = comp.ScaleLerpAmount;
        trail.VelocityLerpAmount = comp.VelocityLerpAmount;
        trail.PositionLerpAmount = comp.PositionLerpAmount;
        trail.AlphaLerpTarget = comp.AlphaLerpTarget;
        trail.ScaleLerpTarget = comp.ScaleLerpTarget;
        trail.Sprite = comp.Sprite;
        trail.Color = comp.Color;
        trail.Scale = comp.Scale;
        trail.TrailData = comp.TrailData;
        trail.Shader = comp.Shader;
        trail.ParticleAmount = comp.ParticleAmount;
        trail.StartAngle = comp.StartAngle;
        trail.EndAngle = comp.EndAngle;
        trail.LerpTime = comp.LerpTime;
        trail.LerpAccumulator = comp.LerpAccumulator;
        trail.RenderedEntity = comp.RenderedEntity;
        trail.Velocity = comp.Velocity;
        trail.Radius = comp.Radius;
        trail.MaxParticleAmount = comp.MaxParticleAmount;
        trail.ParticleCount = comp.ParticleCount;
        trail.SpawnPosition = comp.SpawnPosition;
        trail.SpawnEntityPosition = comp.SpawnEntityPosition;
        trail.RenderedEntityRotationStrategy = comp.RenderedEntityRotationStrategy;
        trail.AdditionalLerpData = comp.AdditionalLerpData;
        trail.TrailData.Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<TrailOverlay>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<TrailComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var trail, out var xform))
        {
            if (trail.Lifetime <= 0f)
                continue;

            var (position, rotation) = _transform.GetWorldPositionRotation(xform, _xformQuery);
            trail.LastCoords = new MapCoordinates(position, xform.MapID);

            if (_frozenQuery.HasComp(uid))
                continue;

            Lerp(trail, position, frameTime);

            trail.Accumulator += frameTime;

            // Assuming that lifetime and frequency don't change
            if (trail.Accumulator > trail.Lifetime && trail.Lifetime < trail.Frequency && trail.TrailData.Count > 0)
                trail.TrailData.Clear();

            if (trail.Frequency <= 0f || trail.ParticleAmount < 1 ||
                trail.MaxParticleAmount > 0 && trail.ParticleCount >= trail.MaxParticleAmount)
            {
                if (trail.Accumulator <= trail.Lifetime)
                    continue;

                trail.Accumulator = 0f;

                for (var i = 0; i < Math.Max(1, trail.ParticleAmount); i++)
                {
                    if (trail.TrailData.Count == 0)
                    {
                        if (IsClientSide(uid) && trail.Frequency <= 0f)
                            QueueDel(uid);
                        break;
                    }

                    trail.TrailData.RemoveAt(0);
                }

                continue;
            }

            if (trail.Accumulator <= trail.Frequency)
                continue;

            trail.Accumulator = 0f;

            if (trail.SpawnEntityPosition != null && !Exists(trail.SpawnEntityPosition.Value))
                continue;

            Angle angle;
            if (_physicsQuery.TryComp(uid, out var physics) && physics.LinearVelocity.LengthSquared() > 0)
                angle = physics.LinearVelocity.ToAngle();
            else
                angle = xform.LocalRotation;

            var start = trail.StartAngle + angle;
            var end = trail.EndAngle + angle;

            // It would break if we try to do this with line based trails
            if (trail.ParticleAmount == 1 || trail is { ParticleAmount: > 1, RenderedEntity: null, Sprite: null })
            {
                var direction = new Angle((end.Theta + start.Theta) * 0.5).ToVec();
                SpawnParticle(trail, position, rotation, direction, xform.MapID);
                continue;
            }

            if (trail.ParticleAmount < 1) // Impossible
                continue;

            var angles = LinearSpread(start, end, trail.ParticleAmount);
            for (var i = 0; i < trail.ParticleAmount; i++)
            {
                SpawnParticle(trail, position, rotation, angles[i].ToVec(), xform.MapID);
                if (trail.MaxParticleAmount > 0 && trail.ParticleCount >= trail.MaxParticleAmount)
                    break;
            }
        }
    }

    private Angle[] LinearSpread(Angle start, Angle end, int intervals)
    {
        DebugTools.Assert(intervals > 1);
        var angles = new Angle[intervals];

        for (var i = 0; i <= intervals - 1; i++)
        {
            angles[i] = new Angle(start + (end - start) * i / (intervals - 1));
        }

        return angles;
    }

    private void SpawnParticle(TrailComponent trail, Vector2 position, Angle rotation, Vector2 direction, MapId mapId)
    {
        DebugTools.Assert(trail is { ParticleAmount: > 0, Frequency: > 0f });
        trail.ParticleCount++;

        if (trail.SpawnEntityPosition != null && Exists(trail.SpawnEntityPosition.Value))
        {
            position = _transform.GetWorldPosition(trail.SpawnEntityPosition.Value, _xformQuery);
            if (trail.SpawnPosition != null)
                position += trail.SpawnPosition.Value;
        }
        else if (trail.SpawnPosition != null)
            position = trail.SpawnPosition.Value;

        var targetPos = position + direction * trail.Radius;
        if (trail.TrailData.Count <
            MathF.Max(trail.ParticleAmount, trail.ParticleAmount * trail.Lifetime / trail.Frequency))
        {
            trail.TrailData.Add(new TrailData(targetPos,
                trail.Velocity,
                mapId,
                direction,
                rotation,
                trail.Color,
                trail.Scale,
                _timing.CurTime));
        }
        else if (trail.TrailData.Count > 0)
        {
            if (trail.CurIndex >= trail.TrailData.Count || trail.Sprite == null)
                trail.CurIndex = 0;

            var data = trail.TrailData[trail.CurIndex];

            data.Color = trail.Color;
            data.Position = targetPos;
            data.Velocity = trail.Velocity;
            data.MapId = mapId;
            data.Direction = direction;
            data.Angle = rotation;
            data.Scale = trail.Scale;
            data.SpawnTime = _timing.CurTime;

            if (trail.Sprite == null)
            {
                if (trail is
                    {
                        AlphaLerpAmount: <= 0f, ScaleLerpAmount: <= 0f, VelocityLerpAmount: <= 0f, Velocity: 0f,
                        PositionLerpAmount: <= 0f,
                    })
                    return;

                trail.TrailData.RemoveAt(0);
                trail.TrailData.Add(data);
            }
            else
                trail.CurIndex++;
        }
    }

    private void Lerp(TrailComponent trail, Vector2 position, float frameTime)
    {
        if (trail is
            {
                AlphaLerpAmount: <= 0f, ScaleLerpAmount: <= 0f, Velocity: 0f, VelocityLerpAmount: <= 0f,
                PositionLerpAmount: <= 0f,
            })
            return;

        trail.LerpAccumulator += frameTime;

        if (trail.LerpAccumulator <= trail.LerpTime)
            return;

        trail.LerpAccumulator = 0;

        foreach (var data in trail.TrailData)
        {
            if (trail.AlphaLerpAmount > 0f)
            {
                var alphaTarget = trail.AlphaLerpTarget is >= 0f and <= 1f ? trail.AlphaLerpTarget : 0f;
                data.Color.A = float.Lerp(data.Color.A, alphaTarget, trail.AlphaLerpAmount);
            }

            if (trail.ScaleLerpAmount > 0f)
            {
                var scaleTarget = trail.ScaleLerpTarget >= 0f ? trail.ScaleLerpTarget : 0f;
                data.Scale = float.Lerp(data.Scale, scaleTarget, trail.ScaleLerpAmount);
            }

            data.Position += data.Direction * data.Velocity;

            if (trail.PositionLerpAmount > 0f)
                data.Position = Vector2.Lerp(data.Position, position, trail.PositionLerpAmount);

            if (trail.VelocityLerpAmount > 0f)
                data.Velocity = float.Lerp(data.Velocity, trail.VelocityLerpTarget, trail.VelocityLerpAmount);
        }

        foreach (var lerpData in trail.AdditionalLerpData.Where(x => x.LerpAmount > 0f))
        {
            lerpData.Value = float.Lerp(lerpData.Value, lerpData.LerpTarget, lerpData.LerpAmount);

            AnimationHelper.SetAnimatableProperty(trail, lerpData.Property, lerpData.Value);
        }
    }
}
