using Content.Goobstation.Common.CCVar;
using Content.Server.Movement.Components;
using Content.Server.Movement.Systems;
using Content.Shared._Trauma.Projectiles;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;

namespace Content.Server._Trauma.Projectiles;

/// <summary>
/// Compensates for shooter's lag by using flyby fixture to check for where the shooter saw the target at time of shooting.
/// Uses flyby fixture as for most entities this will overlap with the client's opinion of where the target "currently" is.
/// </summary>
public sealed class LagCompProjectileSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly LagCompensationSystem _lag = default!;
    [Dependency] private readonly PredictedProjectileSystem _projectile = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private EntityQuery<ActorComponent> _actorQuery;
    private EntityQuery<LagCompensationComponent> _lagQuery;
    private EntityQuery<ProjectileComponent> _projectileQuery;

    /// <summary>
    /// If a projectile is within this distance of a lag-comp'd position for a target, it counts as a hit.
    /// </summary>
    public float Range = 0.6f;

    public override void Initialize()
    {
        base.Initialize();

        _actorQuery = GetEntityQuery<ActorComponent>();
        _lagQuery = GetEntityQuery<LagCompensationComponent>();
        _projectileQuery = GetEntityQuery<ProjectileComponent>();

        SubscribeLocalEvent<PlayerShotProjectileEvent>(OnShotProjectile);
        SubscribeLocalEvent<LagCompProjectileComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<LagCompProjectileComponent, EndCollideEvent>(OnEndCollide);

        Subs.CVar(_cfg, GoobCVars.GunLagCompRange, x => Range = x, true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<LagCompProjectileComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Targets.Count == 0)
                continue;

            // Goob - already hit something this tick (e.g. a wall) and is waiting to be deleted
            if (_projectileQuery.TryComp(uid, out var proj) && proj.ProjectileSpent)
            {
                RemCompDeferred(uid, comp);
                continue;
            }

            var pos = _transform.GetMapCoordinates(uid);
            EntityUid? hit = null;
            foreach (var target in comp.Targets)
            {
                // Goob - don't hit things that got deleted or put in a container since entering the flyby fixture
                if (TerminatingOrDeleted(target) || _container.IsEntityInContainer(target))
                    continue;

                // Goob - old positions can be relative to an entity that got deleted since
                var lagCoords = _lag.GetCoordinates(target, comp.ShooterSession);
                if (!lagCoords.IsValid(EntityManager))
                    continue;

                var lagPos = _transform.ToMapCoordinates(lagCoords);
                if (pos.InRange(lagPos, Range))
                {
                    hit = target;
                    break;
                }
            }

            if (hit is not {} hitTarget)
                continue;

            // it's close to where the client saw target at, do the hit
            // Goob - outside the loop since DoHit can end up modifying Targets
            comp.Targets.Remove(hitTarget);
            _projectile.DoHit(uid, hitTarget);

            // don't need to do lagcomp logic anymore, bullet is being deleted
            // Goob - penetrating bullets keep it, DoHit makes them ignore what they already hit
            if (!_projectileQuery.TryComp(uid, out var projectile) || projectile.ProjectileSpent)
                RemCompDeferred(uid, comp);
        }
    }

    private void OnShotProjectile(ref PlayerShotProjectileEvent args)
    {
        // shooter wasn't player-controlled, don't care
        if (!_actorQuery.TryComp(args.User, out var actor))
            return;

        var session = actor.PlayerSession;

        // add lag comp so it's fairer for high ping chuds
        var comp = EnsureComp<LagCompProjectileComponent>(args.Projectile);
        comp.ShooterSession = session;

        // Goob - the shooter's client predicting the server's projectile is handled by the networked PredictedShooterComponent
    }

    private void OnStartCollide(Entity<LagCompProjectileComponent> ent, ref StartCollideEvent args)
    {
        if (args.OurEntity != ent.Owner || args.OurFixtureId != SharedFlyBySoundSystem.FlyByFixture)
            return;

        var target = args.OtherEntity;
        if (_lagQuery.HasComp(target))
            ent.Comp.Targets.Add(target);
    }

    private void OnEndCollide(Entity<LagCompProjectileComponent> ent, ref EndCollideEvent args)
    {
        if (args.OurEntity != ent.Owner || args.OurFixtureId != SharedFlyBySoundSystem.FlyByFixture)
            return;

        ent.Comp.Targets.Remove(args.OtherEntity);
    }
}
