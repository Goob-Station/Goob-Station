using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Heretic;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;
    [Dependency] protected readonly EntityLookupSystem Lookup = default!;
    [Dependency] protected readonly StatusEffectsSystem Status = default!;
    [Dependency] private readonly StatusEffectNew.StatusEffectsSystem _statusNew = default!;
    [Dependency] private readonly SharedProjectileSystem _projectile = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedStarMarkSystem _starMark = default!;

    [Dependency] protected readonly SharedPopupSystem Popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAsh();
        SubscribeBlade();
        SubscribeRust();
        SubscribeCosmos();
        SubscribeSide();

        SubscribeLocalEvent<HereticComponent, EventHereticShadowCloak>(OnShadowCloak);
    }

    private void OnShadowCloak(Entity<HereticComponent> ent, ref EventHereticShadowCloak args)
    {
        if (!TryComp(ent, out StatusEffectsComponent? status))
            return;

        if (TryComp(ent, out ShadowCloakedComponent? shadowCloaked))
        {
            Status.TryRemoveStatusEffect(ent, args.Status, status, false);
            RemCompDeferred(ent.Owner, shadowCloaked);
            args.Handled = true;
            return;
        }

        // TryUseAbility only if we are not cloaked so that we can uncloak without focus
        // Ideally you should uncloak when losing focus but whatever
        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;
        Status.TryAddStatusEffect<ShadowCloakedComponent>(ent, args.Status, args.Lifetime, true, status);
    }

    protected bool TryUseAbility(EntityUid ent, BaseActionEvent args)
    {
        if (args.Handled)
            return false;

        // No using abilities while charging
        if (HasComp<RustChargeComponent>(ent))
            return false;

        if (!TryComp<HereticActionComponent>(args.Action, out var actionComp))
            return false;

        // check if any magic items are worn
        if (!TryComp<HereticComponent>(ent, out var hereticComp) || !actionComp.RequireMagicItem ||
            hereticComp.Ascended)
        {
            SpeakAbility(ent, actionComp);
            return true;
        }

        var ev = new CheckMagicItemEvent();
        RaiseLocalEvent(ent, ev);

        if (ev.Handled)
        {
            SpeakAbility(ent, actionComp);
            return true;
        }

        // Almost all of the abilites are serverside anyway
        if (_net.IsServer)
            Popup.PopupEntity(Loc.GetString("heretic-ability-fail-magicitem"), ent, ent);

        return false;
    }

    protected void ShootProjectileSpell(EntityUid performer,
        EntityCoordinates coords,
        EntProtoId toSpawn,
        float speed,
        EntityUid? target)
    {
        if (_net.IsClient)
            return;

        var xform = Transform(performer);
        var fromCoords = xform.Coordinates;
        var toCoords = coords;

        var fromMap = _transform.ToMapCoordinates(fromCoords);
        var spawnCoords = _mapMan.TryFindGridAt(fromMap, out var gridUid, out _)
            ? _transform.WithEntityId(fromCoords, gridUid)
            : new(_map.GetMap(fromMap.MapId), fromMap.Position);

        var userVelocity = _physics.GetMapLinearVelocity(spawnCoords);

        var projectile = Spawn(toSpawn, spawnCoords);
        var direction = _transform.ToMapCoordinates(toCoords).Position -
                        _transform.ToMapCoordinates(spawnCoords).Position;
        _gun.ShootProjectile(projectile, direction, userVelocity, performer, performer, speed);

        if (target != null)
            _gun.SetTarget(projectile, target.Value, out _);
    }

    protected virtual void SpeakAbility(EntityUid ent, HereticActionComponent args) { }
}
