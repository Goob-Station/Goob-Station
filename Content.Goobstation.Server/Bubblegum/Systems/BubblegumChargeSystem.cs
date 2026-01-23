using Content.Goobstation.Shared.Bubblegum.Components;
using Content.Goobstation.Shared.Devil;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Goobstation.Server.Bubblegum.Systems;

/// <summary>
/// This is a system that searches for an ActorComponent in the vicinity of the comp holder, then spawns a "target" on top of them, and charges towards it.
/// </summary>
public sealed class BubblegumChargeSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

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

                var chargerPos = Transform(uid).WorldPosition;
                var landingPos = Transform(comp.Landing.Value).WorldPosition;
                var delta = landingPos - chargerPos;

                if (delta.LengthSquared() <= 0.0001f)
                    continue;

                if (delta.LengthSquared() <=
                    comp.LandingReachDistance * comp.LandingReachDistance)
                {
                    _physics.SetLinearVelocity(uid, Vector2.Zero);
                    QueueDel(comp.Landing.Value);

                    StopCharge(uid, comp);
                    continue;
                }

                var direction = delta.Normalized();
                _physics.SetLinearVelocity(uid, direction * comp.ChargeSpeed);
                continue;
            }

            if (_timing.CurTime < comp.NextChargeTime)
                continue;

            TryStartCharge(uid);
            comp.NextChargeTime = _timing.CurTime + comp.ChargeDelay;
            Dirty(uid, comp);
        }
    }

    private void TryStartCharge(EntityUid charger)
    {
        if (!TryComp(charger, out BubblegumChargeComponent? comp))
            return;

        if (comp.IsCharging)
            return;

        var target = FindNearestTarget(charger);
        if (target == null)
            return;

        var landing = SpawnLanding(target.Value, comp.LandingProto);
        comp.Landing = landing;

        StartCharge(charger, comp);
    }

    private EntityUid? FindNearestTarget(EntityUid charger, float maxRange = 20f)
    {
        var origin = Transform(charger).WorldPosition;

        EntityUid? best = null;
        var bestDist = float.MaxValue;

        var query = EntityQueryEnumerator<ActorComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (HasComp<DevilComponent>(uid)) // Devils are excluded because this is a Bubblegum attack. Idk just pretend all lavaland mobs are demons or something, or add a Bubblegum HasComp check later.
                continue;

            var delta = xform.WorldPosition - origin;
            var dist = delta.LengthSquared();

            if (dist > maxRange * maxRange)
                continue;

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
        var targetXform = Transform(target);
        return Spawn(proto, targetXform.Coordinates);
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

        var other = args.OtherEntity;

        if (!HasComp<DamageableComponent>(other))
            return;

        _damageable.TryChangeDamage(
            other,
            comp.Damage,
            targetPart: TargetBodyPart.All);
    }
}
