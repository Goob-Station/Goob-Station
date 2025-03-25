using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Server.Flash;
using Content.Server.Hands.Systems;
using Content.Server.Magic;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Store.Systems;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Heretic;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Store.Components;
using Robust.Shared.Audio.Systems;
using Content.Shared.Popups;
using Robust.Shared.Random;
using Content.Shared.Body.Systems;
using Content.Server.Medical;
using Robust.Server.GameObjects;
using Content.Shared.Stunnable;
using Robust.Shared.Map;
using Content.Shared.StatusEffect;
using Content.Shared.Throwing;
using Content.Server.Station.Systems;
using Content.Shared.Localizations;
using Robust.Shared.Audio;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using Content.Server.Heretic.EntitySystems;
using Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;
using Content.Server.Body.Systems;
using Content.Server.Temperature.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Heretic.Components;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Goobstation.Heretic.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Tag;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem : EntitySystem
{
    // keeping track of all systems in a single file
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly PolymorphSystem _poly = default!;
    [Dependency] private readonly ChainFireballSystem _splitball = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobstate = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly StaminaSystem _stam = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly SharedAudioSystem _aud = default!;
    [Dependency] private readonly DoAfterSystem _doafter = default!;
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly VomitSystem _vomit = default!;
    [Dependency] private readonly PhysicsSystem _phys = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly ProtectiveBladeSystem _pblade = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly VoidCurseSystem _voidcurse = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly RespiratorSystem _respirator = default!;
    [Dependency] private readonly RustbringerSystem _rustbringer = default!;

    private List<EntityUid> GetNearbyPeople(Entity<HereticComponent> ent, float range)
    {
        var list = new List<EntityUid>();
        var lookup = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, range);

        foreach (var look in lookup)
        {
            // ignore heretics with the same path*, affect everyone else
            if ((TryComp<HereticComponent>(look, out var th) && th.CurrentPath == ent.Comp.CurrentPath)
            || HasComp<GhoulComponent>(look))
                continue;

            if (!HasComp<StatusEffectsComponent>(look))
                continue;

            list.Add(look);
        }
        return list;
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticComponent, EventHereticOpenStore>(OnStore);
        SubscribeLocalEvent<HereticComponent, EventHereticMansusGrasp>(OnMansusGrasp);

        SubscribeLocalEvent<HereticComponent, EventHereticLivingHeart>(OnLivingHeart);
        SubscribeLocalEvent<HereticComponent, EventHereticLivingHeartActivate>(OnLivingHeartActivate);

        SubscribeLocalEvent<GhoulComponent, EventHereticMansusLink>(OnMansusLink);
        SubscribeLocalEvent<GhoulComponent, HereticMansusLinkDoAfter>(OnMansusLinkDoafter);

        SubscribeAsh();
        SubscribeFlesh();
        SubscribeVoid();
        SubscribeBlade();
        SubscribeLock();
        SubscribeRust();
        SubscribeSide();
    }

    private bool TryUseAbility(EntityUid ent, BaseActionEvent args)
    {
        if (args.Handled)
            return false;

        if (!TryComp<HereticActionComponent>(args.Action, out var actionComp))
            return false;

        // check if any magic items are worn
        if (TryComp<HereticComponent>(ent, out var hereticComp) && actionComp.RequireMagicItem && !hereticComp.Ascended)
        {
            var ev = new CheckMagicItemEvent();
            RaiseLocalEvent(ent, ev);

            if (!ev.Handled)
            {
                _popup.PopupEntity(Loc.GetString("heretic-ability-fail-magicitem"), ent, ent);
                return false;
            }
        }

        // shout the spell out
        if (!string.IsNullOrWhiteSpace(actionComp.MessageLoc))
            _chat.TrySendInGameICMessage(ent, Loc.GetString(actionComp.MessageLoc!), InGameICChatType.Speak, false);

        return true;
    }
    private void OnStore(Entity<HereticComponent> ent, ref EventHereticOpenStore args)
    {
        if (!TryComp<StoreComponent>(ent, out var store))
            return;

        _store.ToggleUi(ent, ent, store);
    }
    private void OnMansusGrasp(Entity<HereticComponent> ent, ref EventHereticMansusGrasp args)
    {
        if (!TryUseAbility(ent, args))
            return;

        if (ent.Comp.MansusGrasp != EntityUid.Invalid)
        {
            if(!TryComp<HandsComponent>(ent, out var handsComp))
                return;
            foreach (var hand in handsComp.Hands.Values)
            {
                if (hand.HeldEntity == null)
                    continue;
                if (HasComp<MansusGraspComponent>(hand.HeldEntity))
                    QueueDel(hand.HeldEntity);
            }
            ent.Comp.MansusGrasp = EntityUid.Invalid;
            return;
        }

        var st = Spawn(GetMansusGraspProto(ent), Transform(ent).Coordinates);

        if (!_hands.TryForcePickupAnyHand(ent, st))
        {
            _popup.PopupEntity(Loc.GetString("heretic-ability-fail"), ent, ent);
            QueueDel(st);
            return;
        }

        ent.Comp.MansusGrasp = args.Action.Owner;
        args.Handled = true;
    }

    private string GetMansusGraspProto(Entity<HereticComponent> ent)
    {
        if (ent.Comp is { CurrentPath: "Rust", PathStage: >= 2 })
            return "TouchSpellMansusRust";

        return "TouchSpellMansus";
    }

    private void OnLivingHeart(Entity<HereticComponent> ent, ref EventHereticLivingHeart args)
    {
        if (!TryUseAbility(ent, args))
            return;

        if (!TryComp<UserInterfaceComponent>(ent, out var uic))
            return;

        if (ent.Comp.SacrificeTargets.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("heretic-livingheart-notargets"), ent, ent);
            args.Handled = true;
            return;
        }

        _ui.OpenUi((ent, uic), HereticLivingHeartKey.Key, ent);
        args.Handled = true;
    }
    private void OnLivingHeartActivate(Entity<HereticComponent> ent, ref EventHereticLivingHeartActivate args)
    {
        var loc = string.Empty;

        var target = GetEntity(args.Target);
        if (target == null)
            return;

        if (!TryComp<MobStateComponent>(target, out var mobstate))
            return;
        var state = mobstate.CurrentState;
        var locstate = state.ToString().ToLower();

        var ourMapCoords = _transform.GetMapCoordinates(ent);
        var targetMapCoords = _transform.GetMapCoordinates(target.Value);

        if (_map.IsPaused(targetMapCoords.MapId))
            loc = Loc.GetString("heretic-livingheart-unknown");
        else if (targetMapCoords.MapId != ourMapCoords.MapId)
            loc = Loc.GetString("heretic-livingheart-faraway", ("state", locstate));
        else
        {
            var targetStation = _station.GetOwningStation(target);
            var ownStation = _station.GetOwningStation(ent);

            var isOnStation = targetStation != null && targetStation == ownStation;

            var ang = Angle.Zero;
            if (_mapMan.TryFindGridAt(_transform.GetMapCoordinates(Transform(ent)), out var grid, out var _))
                ang = Transform(grid).LocalRotation;

            var vector = targetMapCoords.Position - ourMapCoords.Position;
            var direction = (vector.ToWorldAngle() - ang).GetDir();

            var locdir = ContentLocalizationManager.FormatDirection(direction).ToLower();

            loc = Loc.GetString(isOnStation ? "heretic-livingheart-onstation" : "heretic-livingheart-offstation",
                ("state", locstate),
                ("direction", locdir));
        }

        _popup.PopupEntity(loc, ent, ent, PopupType.Medium);
        _aud.PlayPvs(new SoundPathSpecifier("/Audio/_Goobstation/Heretic/heartbeat.ogg"), ent, AudioParams.Default.WithVolume(-3f));
    }

    public ProtoId<TagPrototype> MansusLinkTag = "MansusLinkMind";
    private void OnMansusLink(Entity<GhoulComponent> ent, ref EventHereticMansusLink args)
    {
        if (!TryUseAbility(ent, args))
            return;

        if (!HasComp<MindContainerComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("heretic-manselink-fail-nomind"), ent, ent);
            return;
        }

        if (_tag.HasTag(args.Target, MansusLinkTag))
        {
            _popup.PopupEntity(Loc.GetString("heretic-manselink-fail-exists"), ent, ent);
            return;
        }

        var dargs = new DoAfterArgs(EntityManager, ent, 5f, new HereticMansusLinkDoAfter(args.Target), ent, args.Target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            MultiplyDelay = false
        };
        _popup.PopupEntity(Loc.GetString("heretic-manselink-start"), ent, ent);
        _popup.PopupEntity(Loc.GetString("heretic-manselink-start-target"), args.Target, args.Target, PopupType.MediumCaution);
        _doafter.TryStartDoAfter(dargs);
    }
    private void OnMansusLinkDoafter(Entity<GhoulComponent> ent, ref HereticMansusLinkDoAfter args)
    {
        if (args.Cancelled)
            return;

        _tag.AddTag(ent, MansusLinkTag);

        // this "* 1000f" (divided by 1000 in FlashSystem) is gonna age like fine wine :clueless:
        _flash.Flash(args.Target, null, null, 2f * 1000f, 0f, false, true, stunDuration: TimeSpan.FromSeconds(1f));
    }
}
