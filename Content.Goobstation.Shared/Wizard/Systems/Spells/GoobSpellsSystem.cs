
using System.Numerics;
using Content.Goobstation.Common.Religion;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Goobstation.Wizard.SpellCards;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Explosion.EntitySystems;
using Content.Shared.Ghost;
using Content.Shared.Magic;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wizard.Systems;

/// <summary>
/// TODO: finish moving goob wiz spells then remove Goob after deleting SpellsSystem
/// </summary>
public sealed partial class SharedGoobSpellsSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedMagicSystem _magic = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedExplosionSystem _explosion = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    private EntityQuery<SpectralComponent> _spectralQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    private LocId _locFailSilicon = "spell-fail-target-silicon";
    private LocId _locFailNotDead = "spell-fail-not-dead";
    private LocId _locFailHomingNoTargets = "spell-fail-no-targets";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ScreamForMeEvent>(OnScreamForMe);
        SubscribeLocalEvent<CorpseExplosionEvent>(OnCorpseExplosion);
        SubscribeLocalEvent<HomingToolboxEvent>(OnHomingToolbox);
        SubscribeLocalEvent<MagicMissileEvent>(OnMagicMissile);

        _spectralQuery = GetEntityQuery<SpectralComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
    }

    private bool IsTouchSpellDenied(EntityUid target)
    {
        var ev = new BeforeCastTouchSpellEvent(target);
        RaiseLocalEvent(target, ev, true);

        return ev.Cancelled;
    }

    private void SpawnHomingProjectile(EntProtoId proto,
        EntityCoordinates coords,
        EntityUid? target,
        EntityUid user,
        MapCoordinates mapCoords,
        Vector2 velocity,
        float speed,
        bool checkMobState,
        MapCoordinates? toCoords = null)
    {
        if (target == null && toCoords == null)
            return;

        var targetPos = toCoords?.Position ?? _xform.GetMapCoordinates(target!.Value).Position;

        var direction = targetPos - mapCoords.Position;
        if (direction == Vector2.Zero)
            return;

        var projectile = PredictedSpawnAtPosition(proto, coords);

        _gun.ShootProjectile(projectile, direction, velocity, user, user, speed);

        if (target == null || target == user || checkMobState && !HasComp<MobStateComponent>(target))
            return;

        _gun.SetTarget(projectile, target, out var targeted, false);

        var homing = EnsureComp<HomingProjectileComponent>(projectile);
        homing.Target = target;

        Entity<HomingProjectileComponent, TargetedProjectileComponent> ent = (projectile, homing, targeted);

        Dirty(ent);
    }

    private (EntityCoordinates coords, MapCoordinates mapCoords, EntityCoordinates spawnCoords, Vector2 velocity)
        GetProjectileData(EntityUid shooter)
    {
        var coords = Transform(shooter).Coordinates;
        var mapCoords = _xform.ToMapCoordinates(coords);

        // If applicable, this ensures the projectile is parented to grid on spawn, instead of the map.
        var spawnCoords = _mapManager.TryFindGridAt(mapCoords, out var gridUid, out _)
            ? _xform.WithEntityId(coords, gridUid)
            : new(_map.GetMapOrInvalid(mapCoords.MapId), mapCoords.Position);

        var velocity = _physics.GetMapLinearVelocity(spawnCoords);

        return (coords, mapCoords, spawnCoords, velocity);
    }

    private bool ValidateLockOnAction(WorldTargetActionEvent ev)
    {
        if (!TryComp(ev.Action.Owner, out LockOnMarkActionComponent? lockOnMark))
            return false;

        if (!_xformQuery.TryComp(ev.Entity, out var xform))
            return true;

        if (!HasComp<MobStateComponent>(ev.Entity.Value) || !HasComp<DamageableComponent>(ev.Entity.Value))
            return false;

        return _xform.InRange(ev.Target, xform.Coordinates, lockOnMark.LockOnRadius + 1f);
    }
}