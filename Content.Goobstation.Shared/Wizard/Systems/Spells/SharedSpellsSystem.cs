using System.Numerics;
using Content.Goobstation.Common.Religion;
using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.Wizard.Components;
using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage;
using Content.Shared.Emp;
using Content.Shared.Explosion.EntitySystems;
using Content.Shared.Fluids;
using Content.Shared.Ghost;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Jittering;
using Content.Shared.Magic;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.PDA;
using Content.Shared.Popups;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

/// <summary>
/// TODO: finish moving goob wiz spells then remove Goob after deleting SpellsSystem
/// </summary>
public abstract partial class SharedSpellsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly DivineInterventionSystem _divineIntervention = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedMagicSystem _magic = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedExplosionSystem _explosion = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedStutteringSystem _stutter = default!;
    [Dependency] private readonly SharedEmpSystem _emp = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private EntityQuery<SpectralComponent> _spectralQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    private LocId _locFailSilicon = "spell-fail-target-silicon";
    private LocId _locFailNotDead = "spell-fail-not-dead";
    private LocId _locFailHomingNoTargets = "spell-fail-no-targets";
    private LocId _locFailHandsOccupied = "spell-fail-hands-occupied";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ScreamForMeEvent>(OnScreamForMe);
        SubscribeLocalEvent<CorpseExplosionEvent>(OnCorpseExplosion);
        SubscribeLocalEvent<HomingToolboxEvent>(OnHomingToolbox);
        SubscribeLocalEvent<MagicMissileEvent>(OnMagicMissile);
        SubscribeLocalEvent<BananaTouchEvent>(OnBananaTouch);
        SubscribeLocalEvent<DisableTechEvent>(OnDisableTech);
        SubscribeLocalEvent<SmokeSpellEvent>(OnSmoke);
        SubscribeLocalEvent<MimeMalaiseEvent>(OnMimeMalaise);
        SubscribeLocalEvent<ChuuniInvocationsEvent>(OnChuuniInvocations);
        SubscribeLocalEvent<StopTimeEvent>(OnStopTime);
        SubscribeLocalEvent<RathenEvent>(OnRathen);
        SubscribeLocalEvent<RepulseEvent>(OnRepulse);
        SubscribeLocalEvent<BlindSpellEvent>(OnBlind);
        SubscribeLocalEvent<PredictionToggleSpellEvent>(OnPredictionToggle);
        SubscribeLocalEvent<LesserSummonGunsEvent>(OnLesserSummonGuns);
        SubscribeLocalEvent<ArcaneBarrageEvent>(OnArcaneBarrage);
        SubscribeLocalEvent<ThrownLightningEvent>(OnThrownLightning);
        SubscribeLocalEvent<TileToggleSpellEvent>(OnTileToggle);
        SubscribeLocalEvent<SpellCardsEvent>(OnSpellCards);
        SubscribeLocalEvent<SummonSimiansEvent>(OnSummonSimians);

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

    protected (EntityCoordinates coords, MapCoordinates mapCoords, EntityCoordinates spawnCoords, Vector2 velocity)
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

    private void SetGear(EntityUid uid,
        Dictionary<string, EntProtoId> gear,
        bool force = true,
        bool makeUnremoveable = true,
        InventoryComponent? inventoryComponent = null)
    {
        // TODO: test if predicts properly, dont know why it does this
        if (_net.IsClient)
            return;

        if (!Resolve(uid, ref inventoryComponent, false))
            return;

        foreach (var (slot, item) in gear)
        {
            _inventory.TryUnequip(uid, slot, true, force, false, inventoryComponent);

            var ent = Spawn(item, Transform(uid).Coordinates);
            if (!_inventory.TryEquip(uid, ent, slot, true, force, false, inventoryComponent))
            {
                Del(ent);
                continue;
            }

            if (slot == "id" &&
                TryComp(ent, out PdaComponent? pdaComponent) &&
                TryComp<IdCardComponent>(pdaComponent.ContainedId, out var id))
                id.FullName = MetaData(uid).EntityName;

            if (makeUnremoveable && HasComp<ClothingComponent>(ent))
                EnsureComp<UnremoveableComponent>(ent);
        }
    }

    private EntityUid? PredictedSpawnItemInHands(EntityUid user, EntProtoId proto, EntityUid action)
    {
        if (!_hands.TryGetEmptyHand(user, out var hand))
        {
            _popup.PopupClient(Loc.GetString(_locFailHandsOccupied), user);
            return null;
        }

        var item = PredictedSpawnAtPosition(proto, Transform(user).Coordinates);
        if (_hands.TryPickup(user, item, hand, false))
            return item;

        PredictedQueueDel(item);
        _actions.SetCooldown(action, TimeSpan.FromSeconds(0.5)); // TODO: wtf?

        return null;
    }
}