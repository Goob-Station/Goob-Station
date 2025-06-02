// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 thebiggestbruh <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Weapons.DelayedKnockdown;
using Content.Goobstation.Shared.Overlays;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Server.Flash;
using Content.Server.Hands.Systems;
using Content.Server.Magic;
using Content.Server.Polymorph.Systems;
using Content.Server.Store.Systems;
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
using Content.Server.Actions;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Temperature.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Heretic.Components;
using Content.Server.Jittering;
using Content.Server.Speech.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems.Abilities;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Eye.Blinding.Components;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chat;
using Content.Shared.Hands.Components;
using Content.Shared.Heretic.Components;
using Content.Shared.Mech.Components;
using Content.Shared.Mobs;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Standing;
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Tag;
using Robust.Server.Containers;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem : SharedHereticAbilitySystem
{
    // keeping track of all systems in a single file
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly PolymorphSystem _poly = default!;
    [Dependency] private readonly ChainFireballSystem _splitball = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobstate = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly StaminaSystem _stam = default!;
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
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly ProtectiveBladeSystem _pblade = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly VoidCurseSystem _voidcurse = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly RespiratorSystem _respirator = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly MansusGraspSystem _mansusGrasp = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly JitteringSystem _jitter = default!;
    [Dependency] private readonly StutteringSystem _stutter = default!;

    private const float LeechingWalkUpdateInterval = 1f;
    private float _accumulator;

    private List<EntityUid> GetNearbyPeople(Entity<HereticComponent> ent, float range)
    {
        var list = new List<EntityUid>();
        var lookup = _lookup.GetEntitiesInRange<MobStateComponent>(Transform(ent).Coordinates, range);

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

        SubscribeLocalEvent<HereticComponent, HereticVoidVisionEvent>(OnVoidVision);

        SubscribeLocalEvent<GhoulComponent, EventHereticMansusLink>(OnMansusLink);
        SubscribeLocalEvent<GhoulComponent, HereticMansusLinkDoAfter>(OnMansusLinkDoafter);

        SubscribeAsh();
        SubscribeFlesh();
        SubscribeVoid();
        SubscribeLock();
    }

    protected override void SpeakAbility(EntityUid ent, HereticActionComponent actionComp)
    {
        // shout the spell out
        if (!string.IsNullOrWhiteSpace(actionComp.MessageLoc))
            _chat.TrySendInGameICMessage(ent, Loc.GetString(actionComp.MessageLoc!), InGameICChatType.Speak, false);
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

        if (!_hands.TryGetEmptyHand(ent, out var emptyHand))
        {
            // Empowered blades - infuse all of our blades that are currently in our inventory
            if (ent.Comp.CurrentPath == "Blade" && ent.Comp.PathStage >= 7)
            {
                if (!InfuseOurBlades())
                    return;

                _actions.SetCooldown(args.Action, MansusGraspSystem.DefaultCooldown);
                _mansusGrasp.InvokeGrasp(ent, null);
            }

            return;
        }

        var st = Spawn(GetMansusGraspProto(ent), Transform(ent).Coordinates);

        if (!_hands.TryPickup(ent, st, emptyHand, animate: false))
        {
            Popup.PopupEntity(Loc.GetString("heretic-ability-fail"), ent, ent);
            QueueDel(st);
            return;
        }

        ent.Comp.MansusGrasp = args.Action.Owner;
        args.Handled = true;

        return;

        bool InfuseOurBlades()
        {
            var xformQuery = GetEntityQuery<TransformComponent>();
            var containerEnt = ent.Owner;
            if (_container.TryGetOuterContainer(ent, xformQuery.Comp(ent), out var container, xformQuery))
                containerEnt = container.Owner;

            var success = false;
            foreach (var blade in ent.Comp.OurBlades)
            {
                if (!EntityManager.EntityExists(blade))
                    continue;

                if (!_tag.HasTag(blade, "HereticBladeBlade"))
                    continue;

                if (TryComp(blade, out MansusInfusedComponent? infused) &&
                    infused.AvailableCharges >= infused.MaxCharges)
                    continue;

                if (!_container.TryGetOuterContainer(blade, xformQuery.Comp(blade), out var bladeContainer, xformQuery))
                    continue;

                if (bladeContainer.Owner != containerEnt)
                    continue;

                var newInfused = EnsureComp<MansusInfusedComponent>(blade);
                newInfused.AvailableCharges = newInfused.MaxCharges;
                success = true;
            }

            return success;
        }
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
            Popup.PopupEntity(Loc.GetString("heretic-livingheart-notargets"), ent, ent);
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

        Popup.PopupEntity(loc, ent, ent, PopupType.Medium);
        _aud.PlayPvs(new SoundPathSpecifier("/Audio/_Goobstation/Heretic/heartbeat.ogg"), ent, AudioParams.Default.WithVolume(-3f));
    }

    public ProtoId<CollectiveMindPrototype> MansusLinkMind = "MansusLink";
    private void OnMansusLink(Entity<GhoulComponent> ent, ref EventHereticMansusLink args)
    {
        if (!TryUseAbility(ent, args))
            return;

        if (!HasComp<MindContainerComponent>(args.Target))
        {
            Popup.PopupEntity(Loc.GetString("heretic-manselink-fail-nomind"), ent, ent);
            return;
        }

        if (TryComp<CollectiveMindComponent>(args.Target, out var mind) && mind.Channels.Contains(MansusLinkMind))
        {
            Popup.PopupEntity(Loc.GetString("heretic-manselink-fail-exists"), ent, ent);
            return;
        }

        var dargs = new DoAfterArgs(EntityManager, ent, 5f, new HereticMansusLinkDoAfter(args.Target), ent, args.Target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            MultiplyDelay = false
        };
        Popup.PopupEntity(Loc.GetString("heretic-manselink-start"), ent, ent);
        Popup.PopupEntity(Loc.GetString("heretic-manselink-start-target"), args.Target, args.Target, PopupType.MediumCaution);
        _doafter.TryStartDoAfter(dargs);
    }
    private void OnMansusLinkDoafter(Entity<GhoulComponent> ent, ref HereticMansusLinkDoAfter args)
    {
        if (args.Cancelled)
            return;

        EnsureComp<CollectiveMindComponent>(args.Target).Channels.Add(MansusLinkMind);

        // this "* 1000f" (divided by 1000 in FlashSystem) is gonna age like fine wine :clueless:
        _flash.Flash(args.Target, null, null, 2f * 1000f, 0f, false, true, stunDuration: TimeSpan.FromSeconds(1f));
    }

    private void OnVoidVision(Entity<HereticComponent> ent, ref HereticVoidVisionEvent args)
    {
        var thermalVision = _compFactory.GetComponent<ThermalVisionComponent>();
        thermalVision.Color = Color.FromHex("#b4babf");
        thermalVision.LightRadius = 7.5f;
        thermalVision.FlashDurationMultiplier = 1f;
        thermalVision.ActivateSound = null;
        thermalVision.DeactivateSound = null;
        thermalVision.ToggleAction = null;

        AddComp(ent, thermalVision);

        var toggleEvent = new ToggleThermalVisionEvent();
        RaiseLocalEvent(ent, toggleEvent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var rustChargeQuery = EntityQueryEnumerator<RustObjectsInRadiusComponent, TransformComponent>();
        while (rustChargeQuery.MoveNext(out var uid, out var rust, out var xform))
        {
            if (rust.NextRustTime > Timing.CurTime)
                continue;

            rust.NextRustTime = Timing.CurTime + rust.RustPeriod;
            RustObjectsInRadius(_transform.GetMapCoordinates(uid, xform),
                rust.RustRadius,
                rust.TileRune,
                rust.LookupRange);
        }

        _accumulator += frameTime;

        if (_accumulator < LeechingWalkUpdateInterval)
            return;

        _accumulator = 0f;

        var damageableQuery = GetEntityQuery<DamageableComponent>();
        var bloodQuery = GetEntityQuery<BloodstreamComponent>();
        var solutionQuery = GetEntityQuery<SolutionContainerManagerComponent>();
        var temperatureQuery = GetEntityQuery<TemperatureComponent>();
        var staminaQuery = GetEntityQuery<StaminaComponent>();
        var statusQuery = GetEntityQuery<StatusEffectsComponent>();
        var rustbringerQuery = GetEntityQuery<RustbringerComponent>();
        var resiratorQuery = GetEntityQuery<RespiratorComponent>();

        var leechQuery = EntityQueryEnumerator<LeechingWalkComponent, TransformComponent>();
        while (leechQuery.MoveNext(out var uid, out var leech, out var xform))
        {
            RemCompDeferred<DisgustComponent>(uid);

            if (!IsTileRust(xform.Coordinates, out _))
                continue;

            var multiplier = 1f;

            if (rustbringerQuery.HasComp(uid))
            {
                multiplier = leech.AscensuionMultiplier;

                if (resiratorQuery.TryComp(uid, out var respirator))
                    _respirator.UpdateSaturation(uid, respirator.MaxSaturation - respirator.MinSaturation, respirator);
            }

            RemCompDeferred<DelayedKnockdownComponent>(uid);

            if (damageableQuery.TryComp(uid, out var damageable))
            {
                _dmg.TryChangeDamage(uid,
                    leech.ToHeal * multiplier,
                    true,
                    false,
                    damageable,
                    null,
                    false,
                    targetPart: TargetBodyPart.All);
            }

            if (bloodQuery.TryComp(uid, out var blood))
            {
                if (blood.BleedAmount > 0f)
                    _blood.TryModifyBleedAmount(uid, -blood.BleedAmount, blood);

                if (solutionQuery.TryComp(uid, out var sol) &&
                    _solution.ResolveSolution((uid, sol), blood.BloodSolutionName, ref blood.BloodSolution) &&
                    blood.BloodSolution.Value.Comp.Solution.Volume < blood.BloodMaxVolume)
                {
                    _blood.TryModifyBloodLevel(uid,
                        FixedPoint2.Min(leech.BloodHeal * multiplier,
                            blood.BloodMaxVolume - blood.BloodSolution.Value.Comp.Solution.Volume),
                        blood);
                }
            }

            if (temperatureQuery.TryComp(uid, out var temperature))
                _temperature.ForceChangeTemperature(uid, leech.TargetTemperature, temperature);

            if (staminaQuery.TryComp(uid, out var stamina) && stamina.StaminaDamage > 0)
            {
                _stam.TakeStaminaDamage(uid,
                    -float.Min(leech.StaminaHeal * multiplier, stamina.StaminaDamage),
                    stamina,
                    visual: false);
            }

            if (statusQuery.TryComp(uid, out var status))
            {
                var reduction = leech.StunReduction * multiplier;
                _statusEffect.TryRemoveTime(uid, "Stun", reduction, status);
                _statusEffect.TryRemoveTime(uid, "KnockedDown", reduction, status);

                _statusEffect.TryRemoveStatusEffect(uid, "Pacified", status);
                _statusEffect.TryRemoveStatusEffect(uid, "ForcedSleep", status);
                _statusEffect.TryRemoveStatusEffect(uid, "SlowedDown", status);
                _statusEffect.TryRemoveStatusEffect(uid, "BlurryVision", status);
                _statusEffect.TryRemoveStatusEffect(uid, "TemporaryBlindness", status);
                _statusEffect.TryRemoveStatusEffect(uid, "SeeingRainbows", status);
            }
        }

        var siliconQuery = GetEntityQuery<SiliconComponent>();
        var borgChassisQuery = GetEntityQuery<BorgChassisComponent>();
        var godmodeQuery = GetEntityQuery<GodmodeComponent>();
        var hereticQuery = GetEntityQuery<HereticComponent>();
        var ghoulQuery = GetEntityQuery<GhoulComponent>();
        var mobQuery = GetEntityQuery<MobStateComponent>();
        var mechQuery = GetEntityQuery<MechComponent>();

        var siliconDamage = new DamageSpecifier(_prot.Index<DamageGroupPrototype>("Brute"), 10);

        var disgustQuery = EntityQueryEnumerator<DisgustComponent, TransformComponent>();
        while (disgustQuery.MoveNext(out var uid, out var disgust, out var xform))
        {
            if (godmodeQuery.HasComp(uid) || hereticQuery.HasComp(uid) || ghoulQuery.HasComp(uid))
            {
                RemCompDeferred(uid, disgust);
                continue;
            }

            var isNotDead = mobQuery.TryComp(uid, out var mobState) && mobState.CurrentState != MobState.Dead;
            var isMech = mechQuery.HasComp(uid);
            var isSilicon = siliconQuery.HasComp(uid) || borgChassisQuery.HasComp(uid) || _tag.HasTag(uid, "Bot");

            // If we are standing on rusted tile while we are a mech or not dead - apply/accumulate rust effects,
            // Else we stop damaging the entity if we are silicon or mech or reduce disgust level.
            if ((isNotDead || isMech) && IsTileRust(xform.Coordinates, out _))
            {
                // Apply rust corruption
                if (isSilicon || isMech)
                {
                    _dmg.TryChangeDamage(uid,
                        siliconDamage,
                        ignoreResistances: true,
                        targetPart: TargetBodyPart.Chest);

                    // Don't popup to mech
                    if (isMech)
                        continue;

                    Popup.PopupEntity(Loc.GetString("rust-corruption-silicon-damage"),
                        uid,
                        uid,
                        PopupType.MediumCaution);

                    continue;
                }

                disgust.CurrentLevel += disgust.ModifierPerUpdate;
            }
            else
            {
                if (isSilicon || isMech)
                {
                    RemCompDeferred(uid, disgust);
                    continue;
                }

                disgust.CurrentLevel -= disgust.PassiveReduction;

                if (disgust.CurrentLevel <= 0f)
                {
                    RemCompDeferred(uid, disgust);
                    continue;
                }
            }

            if (!statusQuery.TryComp(uid, out var status))
                continue;

            // First level: Visual effects. Jitter stutter and popups.
            if (disgust.CurrentLevel >= disgust.NegativeThreshold)
            {
                if (_random.Prob(disgust.NegativeEffectProb))
                {
                    _jitter.DoJitter(uid, disgust.NegativeTime, true, 10f, 10f, true, status);
                    _stutter.DoStutter(uid, disgust.NegativeTime, true, status);
                    Popup.PopupEntity(Loc.GetString("disgust-effect-warning"), uid, uid, PopupType.SmallCaution);
                }
            }

            // Second level: Chance to vomit which knocks down for a long time and reduces disgust level
            if (disgust.CurrentLevel >= disgust.VomitThreshold)
            {
                var vomitProb = Math.Clamp(0.025f + 0.00025f * disgust.VomitThreshold, 0f, 1f);
                if (_random.Prob(vomitProb))
                {
                    _vomit.Vomit(uid);
                    _stun.KnockdownOrStun(uid, disgust.VomitKnockdownTime, true, status);
                    disgust.CurrentLevel -= disgust.VomitThreshold;
                }
            }

            // Third level: Harmful negative effects: eyeblur and slowdown.
            if (disgust.CurrentLevel >= disgust.BadNegativeThreshold)
            {
                if (_random.Prob(disgust.BadNegativeEffectProb))
                {
                    _statusEffect.TryAddStatusEffect<BlurryVisionComponent>(uid,
                        "BlurryVision",
                        disgust.BadNegativeTime,
                        true,
                        status);

                    _stun.TrySlowdown(uid,
                        disgust.BadNegativeTime,
                        true,
                        disgust.SlowdownMultiplier,
                        disgust.SlowdownMultiplier,
                        status);
                }
            }
        }
    }
}
