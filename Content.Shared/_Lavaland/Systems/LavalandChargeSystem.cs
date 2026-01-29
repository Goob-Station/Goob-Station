using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Lavaland.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Shared._Lavaland.Systems;

/// <summary>
/// Handles charging behavior for entities: finds targets using an EntityWhitelist,
/// spawns landing markers, and moves toward them until reached or invalid.
/// </summary>
public sealed class BubblegumChargeSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<LavalandChargeComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.IsCharging && comp.Landing != null)
            {
                if (!Exists(comp.Landing.Value) ||
                    Transform(uid).MapID != Transform(comp.Landing.Value).MapID)
                {
                    StopCharge(uid, comp);
                    continue;
                }

                var delta = Transform(comp.Landing.Value).WorldPosition - Transform(uid).WorldPosition;

                if (delta.Length() <= comp.LandingReachDistance)
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

            if (TryStartCharge(uid, comp))
                comp.NextChargeTime = _timing.CurTime + comp.ChargeDelay;
        }
    }

    #region Target Search

    private bool TryFindNearestTarget(EntityUid charger, LavalandChargeComponent comp, out EntityUid? found)
    {
        found = null;
        var originXform = Transform(charger);
        var originCoords = originXform.Coordinates;

        var bestDist = float.MaxValue;

        foreach (var uid in _lookup.GetEntitiesInRange(originCoords, comp.TargetSearchRange))
        {
            if (!_whitelist.IsWhitelistPass(comp.TargetWhitelist, uid))
                continue;

            var delta = Transform(uid).WorldPosition - originXform.WorldPosition;
            var dist = delta.LengthSquared();

            if (dist < bestDist)
            {
                bestDist = dist;
                found = uid;
            }
        }

        return found != null;
    }

    #endregion

    #region Charge Start/Stop

    public bool TryStartCharge(EntityUid charger, LavalandChargeComponent comp)
    {
        if (comp.IsCharging)
            return false;

        if (!TryFindNearestTarget(charger, comp, out var target) || target == null)
            return false;

        if (_net.IsClient)
            return false;

        comp.Landing = Spawn(comp.LandingProto, Transform(target.Value).Coordinates);
        StartCharge(charger, comp);
        return true;
    }

    public bool TryStartCharge(EntityUid charger, LavalandChargeComponent comp, EntityUid target)
    {
        if (comp.IsCharging)
            return false;

        comp.Landing = Spawn(comp.LandingProto, Transform(target).Coordinates);
        StartCharge(charger, comp);
        return true;
    }

    public bool TryStartCharge(EntityUid charger, LavalandChargeComponent comp, EntityCoordinates coords)
    {
        if (comp.IsCharging)
            return false;

        comp.Landing = Spawn(comp.LandingProto, coords);
        StartCharge(charger, comp);
        return true;
    }

    private void StartCharge(EntityUid charger, LavalandChargeComponent comp)
    {
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
    }

    private void StopCharge(EntityUid uid, LavalandChargeComponent comp)
    {
        comp.IsCharging = false;
        comp.Landing = null;

        RemComp<TrailComponent>(uid);
    }

    #endregion
}
