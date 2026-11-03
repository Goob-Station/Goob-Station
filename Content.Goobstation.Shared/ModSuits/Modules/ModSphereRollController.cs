using System.Numerics;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Damage.Systems;
using Content.Shared.Friction;
using Content.Shared.Gravity;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModSphereRollController : VirtualController
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private ITileDefinitionManager _tileDefs = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedGravitySystem _gravity = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private StandingStateSystem _standing = default!;
    [Dependency] private EntityQuery<MapGridComponent> _gridQuery = default!;
    [Dependency] private EntityQuery<ModSphereTransformModuleComponent> _moduleQuery = default!;
    [Dependency] private EntityQuery<ModSphereDrillComponent> _drillQuery = default!;
    [Dependency] private EntityQuery<PullableComponent> _pullableQuery = default!;
    [Dependency] private EntityQuery<TileFrictionModifierComponent> _frictionQuery = default!;

    private const float HitForgetRange = 1.5f;
    private const float KnockdownCone = 0.5f;
    private const float DrillCone = 0.55f;
    private const float DrillSkin = 0.05f;

    private readonly HashSet<Entity<MobStateComponent>> _mobs = new();
    private readonly HashSet<Entity<PhysicsComponent>> _statics = new();
    private readonly List<(EntityUid Rock, EntityUid Drill, EntityUid User)> _pendingDrills = new();

    public override void Initialize()
    {
        UpdatesAfter.Add(typeof(TileFrictionController));
        base.Initialize();
    }

    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        var query = EntityQueryEnumerator<ModSphereFormComponent, PhysicsComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var form, out var body, out var xform))
        {
            if (prediction && !body.Predict || !_moduleQuery.TryComp(form.Module, out var module))
                continue;

            var velocity = body.LinearVelocity;
            var dirty = PruneHits((uid, form), xform);

            if (!_gravity.IsWeightless(uid))
            {
                Steer((uid, form), module, frameTime, ref velocity);
                ApplyRollingResistance(uid, module, xform, frameTime, ref velocity);

                var speed = velocity.Length();
                if (speed > 0.01f)
                {
                    var worldDirection = velocity / speed;
                    var position = TransformSystem.GetWorldPosition(xform);
                    dirty |= KnockdownMobs((uid, form), module, xform.MapID, position, worldDirection, ref velocity);
                    dirty |= Drill((uid, form), module, xform, position, worldDirection, frameTime, ref velocity);
                }
            }

            if (dirty)
                Dirty(uid, form);

            PhysicsSystem.SetLinearVelocity(uid, velocity, body: body);
            form.PreSolveVelocity = velocity;
        }
    }

    public override void UpdateAfterSolve(bool prediction, float frameTime)
    {
        base.UpdateAfterSolve(prediction, frameTime);

        var query = EntityQueryEnumerator<ModSphereFormComponent, PhysicsComponent>();
        while (query.MoveNext(out var uid, out var form, out var body))
        {
            if (prediction && !body.Predict || !_moduleQuery.TryComp(form.Module, out var module))
                continue;

            var after = body.LinearVelocity;
            var lost = form.PreSolveVelocity - after;
            var impact = lost.Length();
            if (impact < module.BounceThreshold)
                continue;

            PhysicsSystem.SetLinearVelocity(uid, after - lost * module.Restitution, body: body);

            if (impact < module.ImpactSoundThreshold)
                continue;

            form.LastImpact = _timing.CurTime;
            form.ImpactStrength = Math.Clamp(impact / module.MaxSpeed, 0f, 1f);
            form.ImpactDirection = lost / impact;
            Dirty(uid, form);
            _audio.PlayPredicted(module.ImpactSound, uid, uid);
        }
    }

    [SubscribeLocalEvent]
    private void OnPreventCollide(Entity<ModSphereFormComponent> ent, ref PreventCollideEvent args)
    {
        if (ent.Comp.Hits.Contains(args.OtherEntity))
            args.Cancelled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pendingDrills.Count == 0)
            return;

        foreach (var (rock, drill, user) in _pendingDrills)
        {
            if (TerminatingOrDeleted(rock) || !_drillQuery.TryComp(drill, out var drillComp))
                continue;

            if (_modSuit.TryGetModuleSuit(drill, out var suit))
                _modSuit.TryUseCharge(suit, drillComp.PowerCost);

            _damageable.TryChangeDamage(rock, drillComp.Damage, origin: user);
        }

        _pendingDrills.Clear();
    }

    private bool CanSteer(EntityUid uid)
    {
        return _mobState.IsAlive(uid)
            && !HasComp<StunnedComponent>(uid)
            && !(_pullableQuery.TryComp(uid, out var pullable) && pullable.BeingPulled);
    }

    private void Steer(Entity<ModSphereFormComponent> ent, ModSphereTransformModuleComponent module, float frameTime, ref Vector2 velocity)
    {
        if (!ent.Comp.Thrusting || !CanSteer(ent))
            return;

        var offset = ent.Comp.SteerOffset;
        var distance = offset.Length();
        if (distance < module.BrakeRadius)
        {
            Decelerate(module.BrakeDeceleration * frameTime, ref velocity);
            return;
        }

        var strength = Math.Clamp(distance / module.FullThrustDistance, module.MinThrust, 1f);
        var speed = velocity.Length();
        var next = velocity + offset / distance * module.Acceleration * strength * frameTime;
        var nextSpeed = next.Length();
        var cap = MathF.Max(module.MaxSpeed, speed);
        if (nextSpeed > cap)
            next *= cap / nextSpeed;

        velocity = next;
    }

    private void ApplyRollingResistance(EntityUid uid, ModSphereTransformModuleComponent module, TransformComponent xform, float frameTime, ref Vector2 velocity)
    {
        Decelerate(module.RollingResistance * GetTileFriction(uid, xform) * frameTime, ref velocity);
    }

    private static void Decelerate(float amount, ref Vector2 velocity)
    {
        var speed = velocity.Length();
        velocity = speed <= amount ? Vector2.Zero : velocity * ((speed - amount) / speed);
    }

    private float GetTileFriction(EntityUid uid, TransformComponent xform)
    {
        if (!_gridQuery.TryComp(xform.GridUid, out var grid))
            return _frictionQuery.TryComp(xform.MapUid, out var mapFriction) ? mapFriction.Modifier : 1f;

        var tile = _map.GetTileRef(xform.GridUid.Value, grid, xform.Coordinates);
        if (tile.Tile.IsEmpty)
            return 1f;

        var modifier = 1f;
        var anchored = _map.GetAnchoredEntities((xform.GridUid.Value, grid), tile.GridIndices);
        while (anchored.MoveNext(out var anchoredUid))
        {
            if (anchoredUid != uid && _frictionQuery.TryComp(anchoredUid, out var friction))
                modifier *= friction.Modifier;
        }

        return ((ContentTileDefinition) _tileDefs[tile.Tile.TypeId]).Friction * modifier;
    }

    private bool PruneHits(Entity<ModSphereFormComponent> ent, TransformComponent xform)
    {
        if (ent.Comp.Hits.Count == 0)
            return false;

        var position = TransformSystem.GetWorldPosition(xform);
        var removed = ent.Comp.Hits.RemoveAll(hit =>
            TerminatingOrDeleted(hit)
            || Transform(hit).MapID != xform.MapID
            || (TransformSystem.GetWorldPosition(hit) - position).LengthSquared() > HitForgetRange * HitForgetRange);

        return removed > 0;
    }

    private bool KnockdownMobs(Entity<ModSphereFormComponent> ent,
        ModSphereTransformModuleComponent module,
        MapId map,
        Vector2 position,
        Vector2 direction,
        ref Vector2 velocity)
    {
        if (velocity.Length() < module.KnockdownSpeed)
            return false;

        var range = module.Radius * 2f;
        var hit = false;
        _mobs.Clear();
        _lookup.GetEntitiesInRange(map, position, range, _mobs);
        foreach (var mob in _mobs)
        {
            if (mob.Owner == ent.Owner
                || ent.Comp.Hits.Contains(mob)
                || !_mobState.IsAlive(mob, mob.Comp)
                || _standing.IsDown(mob.Owner))
                continue;

            var toMob = TransformSystem.GetWorldPosition(mob) - position;
            var distance = toMob.Length();
            if (distance > range || distance > 0.01f && Vector2.Dot(toMob / distance, direction) < KnockdownCone)
                continue;

            ent.Comp.Hits.Add(mob);
            hit = true;
            _stun.TryKnockdown(mob.Owner, module.KnockdownTime, drop: false);
            _audio.PlayPredicted(module.KnockdownSound, mob, ent);
            velocity *= module.KnockdownSpeedMultiplier;
        }

        return hit;
    }

    private bool Drill(Entity<ModSphereFormComponent> ent,
        ModSphereTransformModuleComponent module,
        TransformComponent xform,
        Vector2 position,
        Vector2 direction,
        float frameTime,
        ref Vector2 velocity)
    {
        if (ent.Comp.Drill is not { } drill
            || !_drillQuery.TryComp(drill, out var drillComp)
            || !_modSuit.TryGetModuleSuit(drill, out var suit))
            return false;

        var speed = velocity.Length();
        if (speed < drillComp.MinSpeed)
            return false;

        var reach = module.Radius + speed * frameTime + DrillSkin;
        var drilled = false;
        _statics.Clear();
        _lookup.GetEntitiesInRange(xform.MapID, position, reach + 0.5f, _statics, LookupFlags.Static);
        foreach (var rock in _statics)
        {
            if (ent.Comp.Hits.Contains(rock) || !_whitelist.IsWhitelistPass(drillComp.Whitelist, rock))
                continue;

            var bounds = _lookup.GetWorldAABB(rock);
            var closest = Vector2.Clamp(position, bounds.BottomLeft, bounds.TopRight);
            if ((closest - position).Length() > reach)
                continue;

            var toCenter = bounds.Center - position;
            if (toCenter.LengthSquared() > 0.0001f && Vector2.Dot(Vector2.Normalize(toCenter), direction) < DrillCone)
                continue;

            if (speed < drillComp.MinSpeed || !_modSuit.HasCharge(suit, drillComp.PowerCost))
                break;

            ent.Comp.Hits.Add(rock);
            drilled = true;
            speed -= drillComp.SpeedCost;
            velocity = speed <= 0f ? Vector2.Zero : Vector2.Normalize(velocity) * speed;

            PhysicsSystem.RegenerateContacts(rock.AsNullable());
            _audio.PlayPredicted(drillComp.Sound, rock, ent);

            if (_net.IsServer)
                _pendingDrills.Add((rock, drill, ent));
        }

        return drilled;
    }
}
