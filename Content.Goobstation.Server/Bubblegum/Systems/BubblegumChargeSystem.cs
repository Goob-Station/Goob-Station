using Content.Goobstation.Shared.Bubblegum.Components;
using Content.Goobstation.Shared.Devil;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Mind.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Goobstation.Server.Bubblegum.Systems;

/// <summary>
/// Searches for a nearby player, spawns a landing marker on them,
/// and charges toward it until reached or invalid.
/// </summary>
public sealed class BubblegumChargeSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BubblegumChargeComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<BubblegumChargeComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, BubblegumChargeComponent comp, ref MapInitEvent args)
    {
        comp.NextChargeTime = _timing.CurTime + comp.ChargeDelay;
        Dirty(uid, comp);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<BubblegumChargeComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.IsCharging && comp.Landing != null)
            {
                if (!Exists(comp.Landing.Value))
                {
                    StopCharge(uid, comp);
                    continue;
                }

                var delta =
                    Transform(comp.Landing.Value).WorldPosition -
                    Transform(uid).WorldPosition;

                if (delta.LengthSquared() <=
                    comp.LandingReachDistance * comp.LandingReachDistance)
                {
                    _physics.SetLinearVelocity(uid, Vector2.Zero);
                    QueueDel(comp.Landing.Value);
                    StopCharge(uid, comp);
                    continue;
                }

                _physics.SetLinearVelocity(uid, delta.Normalized() * comp.ChargeSpeed);
                continue;
            }

            if (_timing.CurTime < comp.NextChargeTime)
                continue;

            TryStartCharge(uid, comp);
            comp.NextChargeTime = _timing.CurTime + comp.ChargeDelay;
            Dirty(uid, comp);
        }
    }

    private void TryStartCharge(EntityUid charger, BubblegumChargeComponent comp)
    {
        if (comp.IsCharging)
            return;

        var target = FindNearestTarget(charger, comp);
        if (target == null)
            return;

        comp.Landing = SpawnLanding(target.Value, comp.LandingProto);
        StartCharge(charger, comp);
    }

    private EntityUid? FindNearestTarget(EntityUid charger, BubblegumChargeComponent comp)
    {
        var originXform = Transform(charger);
        var originCoords = originXform.Coordinates;

        EntityUid? best = null;
        var bestDist = float.MaxValue;

        foreach (var uid in _lookup.GetEntitiesInRange(originCoords, comp.TargetSearchRange))
        {
            if (!HasComp<ActorComponent>(uid))
                continue;

            if (!HasComp<MindContainerComponent>(uid))
                continue;

            if (HasComp<DevilComponent>(uid)) // Devils get special privilege
                continue;

            var delta =
                Transform(uid).WorldPosition -
                originXform.WorldPosition;

            var dist = delta.LengthSquared();

            if (dist < bestDist)
            {
                bestDist = dist;
                best = uid;
            }
        }

        return best;
    }

    private EntityUid SpawnLanding(EntityUid target, EntProtoId proto)
    {
        return Spawn(proto, Transform(target).Coordinates);
    }

    private void StartCharge(EntityUid charger, BubblegumChargeComponent comp)
    {
        if (!HasComp<PhysicsComponent>(charger))
            return;

        if (!HasComp<TrailComponent>(charger))
        {
            var trail = EnsureComp<TrailComponent>(charger);
            trail.Frequency = 0.03f;
            trail.Lifetime = 0.6f;
            trail.Radius = 0.2f;
            trail.ParticleAmount = 1;
            trail.Velocity = 0f;
            trail.PositionLerpAmount = 0.3f;
            trail.AlphaLerpAmount = 0.2f;
            trail.AlphaLerpTarget = 0f;
        }

        comp.IsCharging = true;
        Dirty(charger, comp);
    }

    private void StopCharge(EntityUid uid, BubblegumChargeComponent comp)
    {
        comp.IsCharging = false;
        comp.Landing = null;

        RemComp<TrailComponent>(uid);
        Dirty(uid, comp);
    }

    private void OnStartCollide(
        EntityUid uid,
        BubblegumChargeComponent comp,
        ref StartCollideEvent args)
    {
        if (!comp.IsCharging)
            return;

        if (!HasComp<DamageableComponent>(args.OtherEntity))
            return;

        _damageable.TryChangeDamage(
            args.OtherEntity,
            comp.Damage,
            targetPart: TargetBodyPart.All);
    }
}
