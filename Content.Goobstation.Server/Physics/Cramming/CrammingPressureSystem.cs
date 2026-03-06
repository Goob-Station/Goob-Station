using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.Physics.Cramming;
using Content.Goobstation.Shared.Physics.Cramming;
using Content.Server.Destructible;
using Content.Server.Shuttles.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Destructible;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Physics.Cramming;

public sealed class CrammingPressureSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private const float ShuttleVelocityThreshold = 0.1f;
    private const float MinPushDirectionLength = 0.001f;
    private const float AlignmentEarlyExitThreshold = 0.95f;
    private const float MobCountMultiplierFactor = 0.75f;
    private const float MinBurstDirectionLength = 0.01f;
    private const float OpposingDirectionThreshold = -0.5f;
    private const float SpreadAngleMin = -0.3f;
    private const float SpreadAngleMax = 0.3f;
    private const float BurstScaleIncrement = 0.1f;

    private EntityQuery<ShuttleComponent> _shuttleQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;
    private EntityQuery<MobCollisionComponent> _mobQuery;
    private EntityQuery<MobStateComponent> _mobStateQuery;
    private EntityQuery<DestructibleComponent> _destructibleQuery;

    private bool _enabled;
    private int _minMobThreshold;
    private float _baseDamage;
    private float _referenceMass;
    private float _buildupDelay;
    private float _directionTolerance;
    private int _burstThreshold;
    private float _burstImpulse;
    private DamageTypePrototype _bluntDamage = default!;

    private readonly Dictionary<EntityUid, Dictionary<EntityUid, Vector2>> _pressureContributors = new();
    private readonly Dictionary<EntityUid, HashSet<EntityUid>> _mobToTargets = new();
    private readonly List<EntityUid> _targetsToRemove = new();
    private readonly List<EntityUid> _mobsToRemove = new();
    private readonly List<EntityUid> _aliveMobs = new();
    private readonly Dictionary<EntityUid, Vector2> _mobPositionCache = new();

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, GoobCVars.CrammingEnabled, v => _enabled = v, true);
        Subs.CVar(_cfg, GoobCVars.CrammingMinMobThreshold, v => _minMobThreshold = v, true);
        Subs.CVar(_cfg, GoobCVars.CrammingBaseDamage, v => _baseDamage = v, true);
        Subs.CVar(_cfg, GoobCVars.CrammingReferenceMass, v => _referenceMass = v, true);
        Subs.CVar(_cfg, GoobCVars.CrammingBuildupDelay, v => _buildupDelay = v, true);
        Subs.CVar(_cfg, GoobCVars.CrammingDirectionTolerance, v => _directionTolerance = v, true);
        Subs.CVar(_cfg, GoobCVars.CrammingBurstThreshold, v => _burstThreshold = v, true);
        Subs.CVar(_cfg, GoobCVars.CrammingBurstImpulse, v => _burstImpulse = v, true);

        SubscribeLocalEvent<MobCollisionComponent, MobPushDirectionEvent>(OnMobPushDirection);
        SubscribeLocalEvent<DestructibleComponent, DestructionEventArgs>(OnStructureDestroyed);

        _shuttleQuery = GetEntityQuery<ShuttleComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _mobQuery = GetEntityQuery<MobCollisionComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();
        _destructibleQuery = GetEntityQuery<DestructibleComponent>();
        _bluntDamage = _proto.Index<DamageTypePrototype>("Blunt");
    }

    private void OnMobPushDirection(EntityUid uid, MobCollisionComponent component, ref MobPushDirectionEvent args)
    {
        if (!_enabled)
            return;

        var target = GetContactedDestructible(args.Mob, args.Direction)
                     ?? GetContactedCrushableMob(args.Mob, args.Direction);

        if (target is null)
        {
            RemoveMobFromTracking(args.Mob);
            return;
        }

        var xform = Transform(target.Value);
        if (xform.GridUid is { } gridUid &&
            _shuttleQuery.HasComponent(gridUid) &&
            _physicsQuery.TryGetComponent(gridUid, out var gridPhysics) &&
            gridPhysics.LinearVelocity.LengthSquared() > ShuttleVelocityThreshold)
        {
            RemoveMobFromTracking(args.Mob);
            return;
        }

        if (!_pressureContributors.TryGetValue(target.Value, out var contributors))
        {
            contributors = new Dictionary<EntityUid, Vector2>();
            _pressureContributors[target.Value] = contributors;
        }

        contributors[args.Mob] = args.Direction;

        if (!_mobToTargets.TryGetValue(args.Mob, out var targets))
        {
            targets = new HashSet<EntityUid>();
            _mobToTargets[args.Mob] = targets;
        }
        targets.Add(target.Value);
    }

    private EntityUid? GetContactedDestructible(EntityUid mob, Vector2 pushDirection)
        => GetContactedEntity(mob, pushDirection, _destructibleQuery);

    private EntityUid? GetContactedCrushableMob(EntityUid mob, Vector2 pushDirection)
        => GetContactedEntity(mob, pushDirection, _mobQuery, excludeSelf: true);

    private EntityUid? GetContactedEntity<T>(EntityUid mob, Vector2 pushDirection, EntityQuery<T> query, bool excludeSelf = false) where T : IComponent
    {
        if (pushDirection.LengthSquared() < MinPushDirectionLength)
            return null;

        var normalizedDirection = Vector2.Normalize(pushDirection);
        var contacts = _physics.GetContacts(mob);

        EntityUid? bestEntity = null;
        var bestAlignment = _directionTolerance;

        while (contacts.MoveNext(out var contact))
        {
            if (!contact.IsTouching)
                continue;

            var otherEnt = contact.OtherEnt(mob);

            if (!query.HasComponent(otherEnt))
                continue;

            if (excludeSelf && otherEnt == mob)
                continue;

            var transformA = _physics.GetPhysicsTransform(contact.EntityA);
            var transformB = _physics.GetPhysicsTransform(contact.EntityB);
            contact.GetWorldManifold(transformA, transformB, out var normal);

            if (contact.EntityB == mob)
                normal = -normal;

            var alignment = Vector2.Dot(normalizedDirection, normal);

            if (alignment > bestAlignment)
            {
                bestAlignment = alignment;
                bestEntity = otherEnt;
            }

            if (alignment > AlignmentEarlyExitThreshold)
                break;
        }

        return bestEntity;
    }

    private void OnStructureDestroyed(EntityUid uid, DestructibleComponent component, DestructionEventArgs args)
    {
        if (_pressureContributors.TryGetValue(uid, out var contributors))
        {
            foreach (var mob in contributors.Keys)
            {
                if (_mobToTargets.TryGetValue(mob, out var targets))
                    targets.Remove(uid);
            }
        }
        _pressureContributors.Remove(uid);
    }

    private void RemoveMobFromTracking(EntityUid mob)
    {
        if (!_mobToTargets.TryGetValue(mob, out var targets))
            return;

        foreach (var target in targets)
        {
            if (_pressureContributors.TryGetValue(target, out var contributors))
                contributors.Remove(mob);
        }
        targets.Clear();
        _mobToTargets.Remove(mob);
    }

    private bool IsMobAlive(EntityUid mob)
        => !_mobStateQuery.TryGetComponent(mob, out var mobState) || mobState.CurrentState != MobState.Dead;

    private void TryApplyBurst(EntityUid target, Dictionary<EntityUid, Vector2> contributors)
    {
        var targetPos = _transform.GetWorldPosition(target);

        _mobPositionCache.Clear();
        foreach (var mob in _aliveMobs)
        {
            if (!Deleted(mob))
                _mobPositionCache[mob] = _transform.GetWorldPosition(mob);
        }

        if (_mobPositionCache.Count == 0)
            return;

        var centerPos = Vector2.Zero;
        foreach (var pos in _mobPositionCache.Values)
        {
            centerPos += pos;
        }

        centerPos /= _mobPositionCache.Count;

        var burstDirection = targetPos - centerPos;
        if (burstDirection.LengthSquared() < MinBurstDirectionLength)
            burstDirection = _random.NextVector2();
        burstDirection = Vector2.Normalize(burstDirection);

        foreach (var mob in _aliveMobs)
        {
            ApplyBurstToMob(mob, targetPos, centerPos, burstDirection);
        }

        foreach (var mob in contributors.Keys)
        {
            if (_mobToTargets.TryGetValue(mob, out var mobTargets))
                mobTargets.Remove(target);
        }
        contributors.Clear();
    }

    private void ApplyBurstToMob(EntityUid mob, Vector2 targetPos, Vector2 centerPos, Vector2 burstDirection)
    {
        if (!_mobPositionCache.TryGetValue(mob, out var mobPos))
            return;

        if (!_physicsQuery.TryGetComponent(mob, out var physics))
            return;

        var mobToTarget = targetPos - mobPos;
        var centerToTarget = targetPos - centerPos;
        if (mobToTarget.LengthSquared() < MinPushDirectionLength || centerToTarget.LengthSquared() < MinPushDirectionLength)
            return;

        var dotProduct = Vector2.Dot(Vector2.Normalize(mobToTarget), Vector2.Normalize(centerToTarget));

        var direction = dotProduct < OpposingDirectionThreshold
            ? GetPerpendicularDirection(burstDirection)
            : burstDirection;

        direction = ApplySpreadAngle(direction);

        var burstScale = 1f + (_aliveMobs.Count - _burstThreshold) * BurstScaleIncrement;
        var impulse = direction * _burstImpulse * burstScale;
        _physics.ApplyLinearImpulse(mob, impulse * physics.Mass, body: physics);
    }

    private Vector2 GetPerpendicularDirection(Vector2 burstDirection)
    {
        var perpendicular = new Vector2(-burstDirection.Y, burstDirection.X);
        return _random.Prob(0.5f) ? -perpendicular : perpendicular;
    }

    private Vector2 ApplySpreadAngle(Vector2 direction)
    {
        var spreadAngle = _random.NextFloat(SpreadAngleMin, SpreadAngleMax);
        var cos = MathF.Cos(spreadAngle);
        var sin = MathF.Sin(spreadAngle);
        return new Vector2(
            direction.X * cos - direction.Y * sin,
            direction.X * sin + direction.Y * cos
        );
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_enabled)
        {
            _pressureContributors.Clear();
            _mobToTargets.Clear();
            var query = EntityQueryEnumerator<CrammingPressureComponent>();
            while (query.MoveNext(out var uid, out _))
            {
                RemComp<CrammingPressureComponent>(uid);
            }

            return;
        }

        _targetsToRemove.Clear();

        foreach (var (target, contributors) in _pressureContributors)
        {
            if (Deleted(target))
            {
                _targetsToRemove.Add(target);
                continue;
            }

            _mobsToRemove.Clear();
            _aliveMobs.Clear();
            foreach (var mob in contributors.Keys)
            {
                if (Deleted(mob))
                    _mobsToRemove.Add(mob);
                else if (IsMobAlive(mob))
                    _aliveMobs.Add(mob);
            }
            foreach (var mob in _mobsToRemove)
            {
                contributors.Remove(mob);
                if (_mobToTargets.TryGetValue(mob, out var mobTargets))
                {
                    mobTargets.Remove(target);
                    if (mobTargets.Count == 0)
                        _mobToTargets.Remove(mob);
                }
            }

            if (_aliveMobs.Count < _minMobThreshold)
            {
                if (TryComp<CrammingPressureComponent>(target, out var existingPressure))
                {
                    existingPressure.BuildupAccumulator = 0;
                    existingPressure.BuildupComplete = false;
                }
                continue;
            }

            var pressureComp = EnsureComp<CrammingPressureComponent>(target);
            pressureComp.BuildupAccumulator += frameTime;

            if (pressureComp.BuildupAccumulator < _buildupDelay)
                continue;

            pressureComp.BuildupComplete = true;

            var totalMass = 0f;
            foreach (var mob in _aliveMobs)
            {
                if (_physicsQuery.TryGetComponent(mob, out var physics))
                    totalMass += physics.Mass;
            }

            var massRatio = _referenceMass > 0 ? totalMass / _referenceMass : 1f;
            var mobCountMultiplier = 1f + (_aliveMobs.Count - _minMobThreshold) * MobCountMultiplierFactor;
            var damageAmount = _baseDamage * massRatio * mobCountMultiplier;
            var damageSpec = new DamageSpecifier(_bluntDamage, damageAmount);

            _damage.TryChangeDamage(target, damageSpec);

            foreach (var mob in _aliveMobs)
            {
                _damage.TryChangeDamage(mob, damageSpec);
            }

            if (_aliveMobs.Count >= _burstThreshold)
                TryApplyBurst(target, contributors);
        }

        foreach (var target in _targetsToRemove)
        {
            if (_pressureContributors.TryGetValue(target, out var targetContributors))
            {
                foreach (var mob in targetContributors.Keys)
                {
                    if (_mobToTargets.TryGetValue(mob, out var mobTargets))
                        mobTargets.Remove(target);
                }
            }
            _pressureContributors.Remove(target);
        }

        _targetsToRemove.Clear();
        foreach (var (target, contributors) in _pressureContributors)
        {
            if (contributors.Count == 0)
                _targetsToRemove.Add(target);
        }

        foreach (var target in _targetsToRemove)
        {
            _pressureContributors.Remove(target);
        }
    }
}
