using Content.Server.Administration;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Light.Components;
using Content.Server.Mind;
using Content.Server.Pinpointer;
using Content.Server.Power.Components;
using Content.Shared.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Radio.Components;
using Content.Server.Silicons.Laws;
using Content.Server.Station.Systems;
using Content.Server.Store.Systems;
using Content.Shared.SurveillanceCamera.Components;
using Content.Server.VoiceMask;
using Content.Goobstation.Shared.MalfunctionAi;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Atmos;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Chat.RadioIconsEvents;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.VoiceMask;
using Robust.Shared.Player;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Electrocution;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.StationAi;
using Content.Shared.Turrets;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Popups;
using Content.Shared.RCD.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Shared.Verbs;
using System.Numerics;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.MalfunctionAi;

/// <summary>
/// Server-side logic for the Malfunction AI antagonist. Sets up the CPU ability store and
/// handles all malf ability events (APC hack for processing power, machine overload,
/// cyborg subversion, station blackout, station lockdown, and Doomsday device arming).
/// Abilities are bought in the store; using them is then free but limited by cooldowns.
/// </summary>
public sealed partial class MalfunctionAiSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ApcSystem _apc = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly VoiceMaskSystem _voiceMask = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedDeployableTurretSystem _turrets = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly SiliconLawSystem _law = default!;
    [Dependency] private readonly SharedDoorSystem _doors = default!;
    [Dependency] private readonly SharedElectrocutionSystem _electrify = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedStationAiSystem _stationAi = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TurfSystem _turfs = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly EntProtoId OverloadActionId = "ActionMalfOverloadMachine";
    private static readonly EntProtoId HackCyborgActionId = "ActionMalfHackCyborg";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfunctionAiComponent, ComponentInit>(OnMalfInit);
        SubscribeLocalEvent<MalfunctionAiComponent, ComponentShutdown>(OnMalfShutdown);

        SubscribeLocalEvent<MalfunctionAiComponent, MalfOpenStoreEvent>(OnOpenStore);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfOverloadMachineEvent>(OnOverloadMachine);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfHackCyborgEvent>(OnHackCyborg);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfBlackoutEvent>(OnBlackout);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfLockdownEvent>(OnLockdown);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfDoomsdayEvent>(OnDoomsday);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfDetonateRcdsEvent>(OnDetonateRcds);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfGyroscopeEvent>(OnGyroscope);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfDecryptSyndicateKeysEvent>(OnDecryptSyndicateKeys);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfPlasmaFloodEvent>(OnPlasmaFlood);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfShuntToApcEvent>(OnShuntToApc);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfDisableEmergencyLightsEvent>(OnDisableEmergencyLights);
        SubscribeLocalEvent<MalfShuntedAiComponent, MalfReturnToCoreEvent>(OnReturnToCore);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfVoiceModulatorEvent>(OnVoiceModulator);
        SubscribeLocalEvent<MalfunctionAiComponent, TransformSpeakerNameEvent>(OnTransformSpeakerName);
        SubscribeLocalEvent<MalfunctionAiComponent, TransformSpeakerJobIconEvent>(OnTransformSpeakerJobIcon);
        SubscribeLocalEvent<VoiceMaskComponent, VoiceMaskResetNameMessage>(OnVoiceMaskReset);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfCameraMicsEvent>(OnCameraMics);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfCameraUpgradeEvent>(OnCameraUpgrade);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfOpenBorgsUiEvent>(OnOpenBorgsUi);
        SubscribeLocalEvent<MalfunctionAiComponent, MalfTurretUpgradeEvent>(OnTurretUpgrade);
        SubscribeLocalEvent<SurveillanceCameraComponent, ListenEvent>(OnCameraListen);
        SubscribeLocalEvent<SurveillanceCameraComponent, MapInitEvent>(OnCameraMapInit);
        SubscribeLocalEvent<StationAiTurretComponent, MapInitEvent>(OnTurretMapInit);

        // Alt-click interactions: hacking APCs is always available; overloading/hacking via
        // alt-click only once the matching ability has been bought in the store.
        SubscribeLocalEvent<ApcComponent, GetVerbsEvent<AlternativeVerb>>(OnApcAltVerb);
        SubscribeLocalEvent<ApcPowerReceiverComponent, GetVerbsEvent<AlternativeVerb>>(OnMachineAltVerb);
        SubscribeLocalEvent<BorgChassisComponent, GetVerbsEvent<AlternativeVerb>>(OnCyborgAltVerb);
    }

    private void OnMalfInit(Entity<MalfunctionAiComponent> ent, ref ComponentInit args)
    {
        // Set up the ability store on the AI itself. The store BUI is normally declared on entity
        // prototypes in YAML; since the store is added at runtime, register the UI here too,
        // otherwise the store window can never open. Range 0 = no distance limit and input
        // validation must be off: the AI brain sits inside the core container and fails the
        // regular CanInteract checks (same reason the AI laws UI sets requireInputValidation: false).
        _ui.SetUi(ent.Owner, StoreUiKey.Key,
            new InterfaceData("StoreBoundUserInterface", interactionRange: 0f, requireInputValidation: false));

        // The voice-modulator ability reuses the standard voice-mask window; register it here since
        // the mask normally lives on clothing, not a remote AI brain.
        _ui.SetUi(ent.Owner, VoiceMaskUIKey.Key,
            new InterfaceData("VoiceMaskBoundUserInterface", interactionRange: 0f, requireInputValidation: false));

        _ui.SetUi(ent.Owner, MalfBorgsUiKey.Key,
            new InterfaceData("MalfBorgsBoundUserInterface", interactionRange: 0f, requireInputValidation: false));

        var store = EnsureComp<StoreComponent>(ent.Owner);
        store.CurrencyWhitelist.Add(ent.Comp.Currency);
        foreach (var category in ent.Comp.StoreCategories)
        {
            store.Categories.Add(category);
        }

        _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { ent.Comp.Currency, ent.Comp.StartingPower } },
            ent.Owner,
            store);

        ent.Comp.OpenStoreActionEntity = _actions.AddAction(ent.Owner, ent.Comp.OpenStoreAction);
        ent.Comp.OpenBorgsActionEntity = _actions.AddAction(ent.Owner, ent.Comp.OpenBorgsAction);

        EnsureComp<AlertsComponent>(ent.Owner);
        _alerts.ShowAlert(ent.Owner, ent.Comp.PowerAlert);

        SyncPowerMirror(ent, store);
    }

    private void OnMalfShutdown(Entity<MalfunctionAiComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.OpenStoreActionEntity is { } storeAction)
            _actions.RemoveAction(ent.Owner, storeAction);
        ent.Comp.OpenStoreActionEntity = null;

        if (ent.Comp.OpenBorgsActionEntity is { } borgsAction)
            _actions.RemoveAction(ent.Owner, borgsAction);
        ent.Comp.OpenBorgsActionEntity = null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        var query = EntityQueryEnumerator<MalfunctionAiComponent, StoreComponent>();
        while (query.MoveNext(out var uid, out var comp, out var store))
        {
            // Hacked APCs continuously mine CPU for the AI.
            comp.Accumulator += frameTime;
            if (comp.Accumulator >= 1f)
            {
                var ticks = (int) comp.Accumulator;
                comp.Accumulator -= ticks;

                // Refresh the subverted-borgs window while it is open.
                UpdateBorgsUi((uid, comp));

                // Income only flows from hacked APCs that still exist: tearing a hacked APC
                // down removes its MalfHackedApcComponent and with it the AI's mining rate.
                var aliveHackedApcs = EntityManager.Count<MalfHackedApcComponent>();
                if (aliveHackedApcs > 0)
                {
                    var balance = GetBalance((uid, comp), store);
                    var income = FixedPoint2.Min(comp.MaxProcessingPower - balance,
                        comp.CpuPerApcPerSecond * aliveHackedApcs * ticks);
                    if (income > 0)
                    {
                        _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { comp.Currency, income } },
                            uid,
                            store);
                    }
                }
            }

            SyncPowerMirror((uid, comp), store);

            // End an active lockdown.
            if (comp.LockdownEndTime != null && now >= comp.LockdownEndTime)
            {
                EndLockdown((uid, comp));
            }
        }

        // Vents rigged by a plasma flood keep adding plasma to their tile until they expire.
        var ventQuery = EntityQueryEnumerator<MalfPlasmaVentComponent>();
        while (ventQuery.MoveNext(out var ventUid, out var vent))
        {
            if (now >= vent.EndTime)
            {
                RemComp<MalfPlasmaVentComponent>(ventUid);
                continue;
            }

            var mixture = _atmos.GetContainingMixture(ventUid, excite: true);
            mixture?.AdjustMoles(Gas.Plasma, vent.MolesPerSecond * frameTime);
        }

        // Trigger pending machine overloads.
        var overloadQuery = EntityQueryEnumerator<MalfPendingOverloadComponent>();
        while (overloadQuery.MoveNext(out var machine, out var pending))
        {
            if (now < pending.TriggerAt)
                continue;

            _explosion.QueueExplosion(
                machine,
                "Default",
                pending.Intensity,
                pending.Slope,
                pending.MaxTileIntensity,
                canCreateVacuum: true, // Goob-MalfAi: overloads are lethal and can breach hull
                user: pending.Source);

            RemComp<MalfPendingOverloadComponent>(machine);
        }
    }

    private FixedPoint2 GetBalance(Entity<MalfunctionAiComponent> ent, StoreComponent store)
    {
        return store.Balance.TryGetValue(ent.Comp.Currency, out var balance) ? balance : FixedPoint2.Zero;
    }

    /// <summary>
    /// Keeps the networked HUD mirror of the CPU balance in sync with the store.
    /// </summary>
    private void SyncPowerMirror(Entity<MalfunctionAiComponent> ent, StoreComponent store)
    {
        var balance = GetBalance(ent, store);
        if (ent.Comp.ProcessingPower == balance)
            return;

        ent.Comp.ProcessingPower = balance;
        Dirty(ent);
    }

    /// <summary>
    /// Whether the AI has bought the given ability action in the store.
    /// </summary>
    private bool HasMalfAction(EntityUid uid, EntProtoId actionId)
    {
        foreach (var action in _actions.GetActions(uid))
        {
            if (MetaData(action.Owner).EntityPrototype?.ID == actionId.Id)
                return true;
        }

        return false;
    }

    private void OnOpenStore(Entity<MalfunctionAiComponent> ent, ref MalfOpenStoreEvent args)
    {
        if (args.Handled)
            return;

        _store.ToggleUi(ent.Owner, ent.Owner);
        args.Handled = true;
    }

    // --- Alt-click verbs ---

    private void OnApcAltVerb(Entity<ApcComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;
        if (!TryComp<MalfunctionAiComponent>(user, out var malf))
            return;

        if (HasComp<MalfHackedApcComponent>(ent.Owner))
            return;

        var target = ent.Owner;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("malfunction-ai-verb-hack-apc"),
            Priority = 10,
            Act = () => TryHackApc((user, malf), target),
        });
    }

    private void OnMachineAltVerb(Entity<ApcPowerReceiverComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;
        if (!TryComp<MalfunctionAiComponent>(user, out var malf))
            return;

        if (!HasMalfAction(user, OverloadActionId))
            return;

        // Don't overload yourself or APCs (those get hacked instead).
        if (ent.Owner == user || HasComp<ApcComponent>(ent.Owner) || HasComp<MalfPendingOverloadComponent>(ent.Owner))
            return;

        var target = ent.Owner;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("malfunction-ai-verb-overload-machine"),
            Act = () => TryOverloadMachine((user, malf), target),
        });
    }

    private void OnCyborgAltVerb(Entity<BorgChassisComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;
        if (!TryComp<MalfunctionAiComponent>(user, out var malf))
            return;

        if (!HasMalfAction(user, HackCyborgActionId))
            return;

        if (HasComp<MalfHackedCyborgComponent>(ent.Owner))
            return;

        var target = ent.Owner;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("malfunction-ai-verb-hack-cyborg"),
            Act = () => TryHackCyborg((user, malf), target),
        });
    }

    // --- APC hacking (income) ---

    public bool TryHackApc(Entity<MalfunctionAiComponent> ent, EntityUid target)
    {
        if (!HasComp<ApcComponent>(target))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-invalid-target"), ent.Owner);
            return false;
        }

        if (HasComp<MalfHackedApcComponent>(target))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-apc-already-hacked"), ent.Owner);
            return false;
        }

        // Hacking grants processing power rather than costing it.
        AddComp<MalfHackedApcComponent>(target);
        ent.Comp.HackedApcCount++;
        Dirty(ent);

        // Refresh the APC screen right away: hacked APCs show the blue glitched (emag) screen.
        _apc.UpdateApcState(target);

        var gain = FixedPoint2.Zero;
        if (TryComp<StoreComponent>(ent.Owner, out var store))
        {
            var balance = GetBalance(ent, store);
            gain = FixedPoint2.Min(ent.Comp.MaxProcessingPower - balance, ent.Comp.CpuPerApc);
            if (gain > 0)
            {
                _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { ent.Comp.Currency, gain } },
                    ent.Owner,
                    store);
            }

            SyncPowerMirror(ent, store);
        }

        _popups.PopupCursor(
            Loc.GetString("malfunction-ai-popup-hack-apc-success",
                ("gain", gain.Int()),
                ("power", ent.Comp.ProcessingPower.Int()),
                ("count", ent.Comp.HackedApcCount)),
            ent.Owner);
        return true;
    }

    // --- Overload machine ---

    private void OnOverloadMachine(Entity<MalfunctionAiComponent> ent, ref MalfOverloadMachineEvent args)
    {
        if (args.Handled)
            return;

        // Prefer the machine actually under the cursor, then fall back to whatever
        // valid machine is closest to the clicked point — never a random neighbour.
        if (args.Entity is { } hovered
            && IsValidOverloadTarget(ent.Owner, hovered)
            && TryOverloadMachine(ent, hovered))
        {
            args.Handled = true;
            return;
        }

        var clickPos = _transform.ToMapCoordinates(args.Target).Position;
        EntityUid? best = null;
        var bestDistance = float.MaxValue;

        foreach (var candidate in _lookup.GetEntitiesInRange(args.Target, 0.75f))
        {
            if (!IsValidOverloadTarget(ent.Owner, candidate))
                continue;

            var distance = (clickPos - _transform.GetWorldPosition(candidate)).Length();
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        if (best != null && TryOverloadMachine(ent, best.Value))
        {
            args.Handled = true;
            return;
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-invalid-target"), ent.Owner);
    }

    private bool IsValidOverloadTarget(EntityUid user, EntityUid candidate)
    {
        return candidate != user
            && HasComp<ApcPowerReceiverComponent>(candidate)
            && !HasComp<ApcComponent>(candidate)
            && !HasComp<MalfPendingOverloadComponent>(candidate);
    }

    private bool TryOverloadMachine(Entity<MalfunctionAiComponent> ent, EntityUid target)
    {
        if (target == ent.Owner || HasComp<MalfPendingOverloadComponent>(target))
            return false;

        var pending = AddComp<MalfPendingOverloadComponent>(target);
        pending.TriggerAt = _timing.CurTime + ent.Comp.OverloadDelay;
        pending.Intensity = ent.Comp.OverloadIntensity;
        pending.MaxTileIntensity = ent.Comp.OverloadMaxTileIntensity;
        pending.Slope = ent.Comp.OverloadExplosionSlope;
        pending.Source = ent.Owner;

        _audio.PlayPvs(ent.Comp.OverloadWarningSound, target);

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-overload-success"), ent.Owner);
        return true;
    }

    // --- Hack cyborg ---

    private void OnHackCyborg(Entity<MalfunctionAiComponent> ent, ref MalfHackCyborgEvent args)
    {
        if (args.Handled)
            return;

        // Prefer the cyborg actually under the cursor.
        if (args.Entity is { } hovered
            && HasComp<BorgChassisComponent>(hovered)
            && TryHackCyborg(ent, hovered))
        {
            args.Handled = true;
            return;
        }

        foreach (var candidate in _lookup.GetEntitiesInRange(args.Target, 0.75f))
        {
            if (!HasComp<BorgChassisComponent>(candidate))
                continue;

            if (TryHackCyborg(ent, candidate))
            {
                args.Handled = true;
                return;
            }
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-invalid-cyborg"), ent.Owner);
    }

    private bool TryHackCyborg(Entity<MalfunctionAiComponent> ent, EntityUid target)
    {
        if (!HasComp<BorgChassisComponent>(target))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-invalid-cyborg"), ent.Owner);
            return false;
        }

        if (HasComp<MalfHackedCyborgComponent>(target))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-cyborg-already-hacked"), ent.Owner);
            return false;
        }

        // Keep the borg's normal laws but prepend the hidden malfunction law 0, flagging it as an antag.
        // The borg player hears the same malf theme the AI got with its briefing.
        if (!_law.AddMalfunctionLaw(target, ensureSubvertedRole: true, cue: ent.Comp.HackCyborgSound))
        {
            // Already subverted (e.g. emagged).
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-cyborg-already-hacked"), ent.Owner);
            return false;
        }

        AddComp<MalfHackedCyborgComponent>(target);
        ent.Comp.HackedCyborgs.Add(target);
        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-hack-cyborg-success"), ent.Owner);
        return true;
    }

    // --- Blackout ---

    private void OnBlackout(Entity<MalfunctionAiComponent> ent, ref MalfBlackoutEvent args)
    {
        if (args.Handled)
            return;

        var gridUid = Transform(ent.Owner).GridUid;
        var count = 0;

        var query = EntityQueryEnumerator<ApcComponent, TransformComponent>();
        while (query.MoveNext(out var apcUid, out var apc, out var xform))
        {
            if (gridUid != null && xform.GridUid != gridUid)
                continue;

            if (!apc.MainBreakerEnabled)
                continue;

            _apc.ApcToggleBreaker(apcUid, apc);
            count++;
        }

        AnnounceFromAi(ent.Owner, Loc.GetString("malfunction-ai-announcement-blackout"));

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-blackout-success", ("count", count)), ent.Owner);
        args.Handled = true;
    }

    // --- Lockdown ---

    private void OnLockdown(Entity<MalfunctionAiComponent> ent, ref MalfLockdownEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.LockdownEndTime != null)
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-lockdown-active"), ent.Owner);
            return;
        }

        var gridUid = Transform(ent.Owner).GridUid;
        ent.Comp.LockedDoors.Clear();

        var query = EntityQueryEnumerator<DoorBoltComponent, TransformComponent>();
        while (query.MoveNext(out var doorUid, out var bolt, out var xform))
        {
            if (gridUid != null && xform.GridUid != gridUid)
                continue;

            if (!_doors.TrySetBoltDown((doorUid, bolt), true, ent.Owner, predicted: false))
                continue;

            if (TryComp<ElectrifiedComponent>(doorUid, out var electrified))
                _electrify.SetElectrified((doorUid, electrified), true);

            ent.Comp.LockedDoors.Add(doorUid);
        }

        ent.Comp.LockdownEndTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.LockdownDuration);
        Dirty(ent);

        AnnounceFromAi(ent.Owner, Loc.GetString("malfunction-ai-announcement-lockdown"));

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-lockdown-success", ("count", ent.Comp.LockedDoors.Count)), ent.Owner);
        args.Handled = true;
    }

    private void EndLockdown(Entity<MalfunctionAiComponent> ent)
    {
        foreach (var doorUid in ent.Comp.LockedDoors)
        {
            if (TryComp<DoorBoltComponent>(doorUid, out var bolt))
                _doors.TrySetBoltDown((doorUid, bolt), false, ent.Owner, predicted: false);

            if (TryComp<ElectrifiedComponent>(doorUid, out var electrified))
                _electrify.SetElectrified((doorUid, electrified), false);
        }

        ent.Comp.LockedDoors.Clear();
        ent.Comp.LockdownEndTime = null;
        Dirty(ent);
    }

    // --- Doomsday ---

    private void OnDoomsday(Entity<MalfunctionAiComponent> ent, ref MalfDoomsdayEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.DoomsdayUsed)
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-doomsday-already-used"), ent.Owner);
            return;
        }

        // The device can be bought early, but arming it requires enough hacked APCs.
        if (ent.Comp.HackedApcCount < ent.Comp.DoomsdayRequiredApcs)
        {
            _popups.PopupCursor(
                Loc.GetString("malfunction-ai-popup-doomsday-need-apcs",
                    ("required", ent.Comp.DoomsdayRequiredApcs),
                    ("current", ent.Comp.HackedApcCount)),
                ent.Owner);
            return;
        }

        ent.Comp.DoomsdayUsed = true;
        Dirty(ent);

        var doomEv = new MalfDoomsdayArmedEvent(ent.Owner);
        RaiseLocalEvent(ref doomEv);
        args.Handled = true;
    }

    // --- Detonate RCDs ---

    private void OnDetonateRcds(Entity<MalfunctionAiComponent> ent, ref MalfDetonateRcdsEvent args)
    {
        if (args.Handled)
            return;

        var gridUid = Transform(ent.Owner).GridUid;
        var count = 0;

        var query = EntityQueryEnumerator<RCDComponent, TransformComponent>();
        while (query.MoveNext(out var rcdUid, out _, out var xform))
        {
            if (gridUid != null && xform.GridUid != gridUid)
                continue;

            if (HasComp<MalfPendingOverloadComponent>(rcdUid))
                continue;

            var pending = AddComp<MalfPendingOverloadComponent>(rcdUid);
            pending.TriggerAt = _timing.CurTime + ent.Comp.RcdDetonationDelay;
            pending.Intensity = ent.Comp.RcdExplosionIntensity;
            pending.MaxTileIntensity = ent.Comp.RcdMaxTileIntensity;
            pending.Slope = ent.Comp.RcdExplosionSlope;
            pending.Source = ent.Owner;

            _audio.PlayPvs(ent.Comp.RcdWarningSound, rcdUid);
            _popups.PopupEntity(Loc.GetString("malfunction-ai-popup-rcd-warning"), rcdUid, PopupType.LargeCaution);
            count++;
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-rcd-success", ("count", count)), ent.Owner);
        args.Handled = true;
    }

    // --- Gyroscope (move the AI core) ---

    private void OnGyroscope(Entity<MalfunctionAiComponent> ent, ref MalfGyroscopeEvent args)
    {
        if (args.Handled)
            return;

        // Only works while the brain actually sits in a core (not carded).
        if (!_stationAi.TryGetCore(ent.Owner, out var maybeCore) || maybeCore.Comp == null)
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-gyroscope-no-core"), ent.Owner);
            return;
        }

        var core = maybeCore.Owner;
        var coreXform = Transform(core);
        if (coreXform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-gyroscope-blocked"), ent.Owner);
            return;
        }

        // One tile per use, towards the click (8-directional).
        var coreTile = _map.TileIndicesFor(gridUid, grid, coreXform.Coordinates);
        var targetTile = _map.TileIndicesFor(gridUid, grid, args.Target);
        var offset = new Vector2i(Math.Clamp(targetTile.X - coreTile.X, -1, 1),
            Math.Clamp(targetTile.Y - coreTile.Y, -1, 1));

        if (offset == Vector2i.Zero)
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-gyroscope-blocked"), ent.Owner);
            return;
        }

        var destination = coreTile + offset;

        // The destination must be a real floor tile with no walls, windows, tables or machines on it.
        if (!_map.TryGetTileRef(gridUid, grid, destination, out var tileRef)
            || tileRef.Tile.IsEmpty
            || _turfs.IsTileBlocked(gridUid, destination, CollisionGroup.FullTileMask, grid))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-gyroscope-blocked"), ent.Owner);
            return;
        }

        var wasAnchored = coreXform.Anchored;
        if (wasAnchored)
            _transform.Unanchor(core, coreXform);

        _transform.SetCoordinates(core, _map.GridTileToLocal(gridUid, grid, destination));

        if (wasAnchored)
            _transform.AnchorEntity(core, coreXform);

        // Anything alive on the destination tile gets crushed under the core's bulk:
        // bodied creatures are gibbed outright (no corpse left to shove aside), the rest
        // take massive blunt damage.
        var crushed = 0;
        foreach (var victim in _lookup.GetLocalEntitiesIntersecting(gridUid, destination, gridComp: grid))
        {
            if (victim == core || victim == ent.Owner || !HasComp<MobStateComponent>(victim))
                continue;

            _popups.PopupEntity(Loc.GetString("malfunction-ai-popup-gyroscope-crush", ("target", Name(victim))), victim, PopupType.LargeCaution);

            if (TryComp<BodyComponent>(victim, out var body))
                _body.GibBody(victim, gibOrgans: true, body: body);
            else
                _damageable.TryChangeDamage(victim, ent.Comp.GyroscopeCrushDamage, ignoreResistances: true, origin: ent.Owner);

            crushed++;
        }

        if (crushed > 0)
            _audio.PlayPvs(ent.Comp.GyroscopeCrushSound, core);

        _audio.PlayPvs(ent.Comp.OverloadWarningSound, core);
        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-gyroscope-success"), ent.Owner);
        args.Handled = true;
    }

    // --- Plasma flood ---

    private void OnPlasmaFlood(Entity<MalfunctionAiComponent> ent, ref MalfPlasmaFloodEvent args)
    {
        if (args.Handled)
            return;

        var gridUid = Transform(ent.Owner).GridUid;
        var count = 0;

        var query = EntityQueryEnumerator<GasVentPumpComponent, TransformComponent>();
        while (query.MoveNext(out var ventUid, out _, out var xform))
        {
            if (gridUid != null && xform.GridUid != gridUid)
                continue;

            if (!xform.Anchored)
                continue;

            var vent = EnsureComp<MalfPlasmaVentComponent>(ventUid);
            vent.EndTime = _timing.CurTime + ent.Comp.PlasmaFloodDuration;
            vent.MolesPerSecond = ent.Comp.PlasmaMolesPerSecond;
            count++;
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-plasma-flood-success", ("count", count)), ent.Owner);
        args.Handled = true;
    }

    // --- Shunt to APC ---

    private void OnShuntToApc(Entity<MalfunctionAiComponent> ent, ref MalfShuntToApcEvent args)
    {
        if (args.Handled)
            return;

        // Only from inside a core: no escaping out of an intellicard.
        if (!_stationAi.TryGetCore(ent.Owner, out var maybeCore) || maybeCore.Comp == null)
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-gyroscope-no-core"), ent.Owner);
            return;
        }

        EntityUid? apc = null;
        foreach (var candidate in _lookup.GetEntitiesInRange(args.Target, 0.75f))
        {
            if (!HasComp<ApcComponent>(candidate))
                continue;

            apc = candidate;
            break;
        }

        if (apc == null)
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-invalid-target"), ent.Owner);
            return;
        }

        // Consciousness can only run on APCs the AI has already hacked.
        if (!HasComp<MalfHackedApcComponent>(apc.Value))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-shunt-not-hacked"), ent.Owner);
            return;
        }

        if (!_mind.TryGetMind(ent.Owner, out var mindId, out _))
            return;

        // Parented to the APC: if the APC is destroyed, the shunted presence dies with it.
        var shunt = Spawn(ent.Comp.ShuntEntity, new EntityCoordinates(apc.Value, Vector2.Zero));
        EnsureComp<MalfShuntedAiComponent>(shunt).Brain = ent.Owner;

        _mind.TransferTo(mindId, shunt);
        args.Handled = true;
    }

    private void OnReturnToCore(Entity<MalfShuntedAiComponent> ent, ref MalfReturnToCoreEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.Brain is not { } brain || !Exists(brain) || Deleted(brain))
        {
            _popups.PopupEntity(Loc.GetString("malfunction-ai-popup-return-no-core"), ent.Owner, ent.Owner);
            return;
        }

        if (!_mind.TryGetMind(ent.Owner, out var mindId, out _))
            return;

        _mind.TransferTo(mindId, brain);
        QueueDel(ent.Owner);
        args.Handled = true;
    }

    // --- Disable emergency lights ---

    private void OnDisableEmergencyLights(Entity<MalfunctionAiComponent> ent, ref MalfDisableEmergencyLightsEvent args)
    {
        if (args.Handled)
            return;

        var gridUid = Transform(ent.Owner).GridUid;
        var count = 0;

        var query = EntityQueryEnumerator<EmergencyLightComponent, TransformComponent>();
        while (query.MoveNext(out var lightUid, out _, out var xform))
        {
            if (gridUid != null && xform.GridUid != gridUid)
                continue;

            if (!TryComp<BatteryComponent>(lightUid, out var battery))
                continue;

            _battery.SetCharge((lightUid, battery), 0f);
            count++;
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-emergency-lights-success", ("count", count)), ent.Owner);
        args.Handled = true;
    }

    // --- Voice modulator ---
    // Reuses the built-in voice-mask window (name + speech verb + job-icon selector) on the AI itself,
    // so a spoofed radio message no longer shows the giveaway Station AI icon.

    private void OnVoiceModulator(Entity<MalfunctionAiComponent> ent, ref MalfVoiceModulatorEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<ActorComponent>(ent.Owner, out var actor))
            return;

        // The voice mask component holds the chosen name/verb/icon; the mask UI drives it.
        EnsureComp<VoiceMaskComponent>(ent.Owner);
        _ui.OpenUi(ent.Owner, VoiceMaskUIKey.Key, actor.PlayerSession);
        args.Handled = true;
    }

    private void OnTransformSpeakerName(Entity<MalfunctionAiComponent> ent, ref TransformSpeakerNameEvent args)
    {
        // The mask lives directly on the speaker, so its own inventory-relayed handler never fires.
        if (!TryComp<VoiceMaskComponent>(ent.Owner, out var mask))
            return;

        if (mask.VoiceMaskName is { } name)
            args.VoiceName = name;

        if (mask.VoiceMaskSpeechVerb is { } verb)
            args.SpeechVerb = verb;
    }

    private void OnTransformSpeakerJobIcon(Entity<MalfunctionAiComponent> ent, ref TransformSpeakerJobIconEvent args)
    {
        if (!TryComp<VoiceMaskComponent>(ent.Owner, out var mask) || mask.JobIconProtoId is not { } icon)
            return;

        args.JobIcon = icon;
        args.JobName = mask.JobName ?? args.JobName;
    }

    private void OnVoiceMaskReset(Entity<VoiceMaskComponent> ent, ref VoiceMaskResetNameMessage args)
    {
        // Use the real name rather than null: null makes the mask UI show the "Unknown"
        // placeholder, which confused players into thinking the reset was stuck.
        ent.Comp.VoiceMaskName = Name(ent.Owner);
        ent.Comp.VoiceMaskSpeechVerb = null;
        ent.Comp.JobIconProtoId = null;
        ent.Comp.JobName = null;

        _popups.PopupCursor(Loc.GetString("voice-mask-popup-reset"), args.Actor);
        _voiceMask.UpdateUI(ent);
    }

    // --- Camera microphones ---

    private void OnCameraMics(Entity<MalfunctionAiComponent> ent, ref MalfCameraMicsEvent args)
    {
        ent.Comp.CameraMicsUnlocked = true;

        // Existing cameras start listening; new cameras are handled on map init.
        var count = 0;
        var query = EntityQueryEnumerator<SurveillanceCameraComponent>();
        while (query.MoveNext(out var camUid, out _))
        {
            EnsureComp<ActiveListenerComponent>(camUid).Range = ent.Comp.CameraMicListenRange;
            count++;
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-camera-mics-success", ("count", count)), ent.Owner);
    }

    private void OnCameraMapInit(Entity<SurveillanceCameraComponent> ent, ref MapInitEvent args)
    {
        // Newly built cameras inherit whatever malf upgrades are already active.
        var query = EntityQueryEnumerator<MalfunctionAiComponent>();
        while (query.MoveNext(out _, out var malf))
        {
            if (malf.CameraMicsUnlocked)
                EnsureComp<ActiveListenerComponent>(ent.Owner).Range = malf.CameraMicListenRange;

            if (malf.CameraXrayUnlocked && TryComp<StationAiVisionComponent>(ent.Owner, out var vision))
                _stationAi.SetVisionUpgrade((ent.Owner, vision), false, malf.CameraUpgradeRange);
        }
    }

    // --- Camera network upgrade (X-ray) ---

    private void OnCameraUpgrade(Entity<MalfunctionAiComponent> ent, ref MalfCameraUpgradeEvent args)
    {
        ent.Comp.CameraXrayUnlocked = true;

        var count = 0;
        var query = EntityQueryEnumerator<SurveillanceCameraComponent, StationAiVisionComponent>();
        while (query.MoveNext(out var camUid, out _, out var vision))
        {
            _stationAi.SetVisionUpgrade((camUid, vision), false, ent.Comp.CameraUpgradeRange);
            count++;
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-camera-upgrade-success", ("count", count)), ent.Owner);
    }

    // --- Subverted borgs window ---

    private void OnOpenBorgsUi(Entity<MalfunctionAiComponent> ent, ref MalfOpenBorgsUiEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<ActorComponent>(ent.Owner, out var actor))
            return;

        _ui.OpenUi(ent.Owner, MalfBorgsUiKey.Key, actor.PlayerSession);
        UpdateBorgsUi(ent);
        args.Handled = true;
    }

    private void UpdateBorgsUi(Entity<MalfunctionAiComponent> ent)
    {
        if (!_ui.IsUiOpen(ent.Owner, MalfBorgsUiKey.Key))
            return;

        var entries = new List<MalfBorgEntry>();
        foreach (var borg in ent.Comp.HackedCyborgs)
        {
            if (!Exists(borg) || Deleted(borg))
                continue;

            entries.Add(new MalfBorgEntry
            {
                Name = Name(borg),
                Alive = _mobState.IsAlive(borg),
                Location = _navMap.GetNearestBeaconString((borg, Transform(borg))),
            });
        }

        _ui.SetUiState(ent.Owner, MalfBorgsUiKey.Key, new MalfBorgsBuiState(entries));
    }

    // --- AI turret upgrade ---

    private void OnTurretUpgrade(Entity<MalfunctionAiComponent> ent, ref MalfTurretUpgradeEvent args)
    {
        ent.Comp.TurretsUpgraded = true;

        var count = 0;
        var query = EntityQueryEnumerator<StationAiTurretComponent>();
        while (query.MoveNext(out var turretUid, out _))
        {
            ApplyTurretUpgrade(turretUid, ent.Comp);
            count++;
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-turrets-success", ("count", count)), ent.Owner);
    }

    private void ApplyTurretUpgrade(EntityUid turret, MalfunctionAiComponent malf)
    {
        // Power: the turret shoots faster.
        if (TryComp<GunComponent>(turret, out var gun))
        {
            gun.FireRate *= malf.TurretFireRateMultiplier;
            Dirty(turret, gun);
            _gun.RefreshModifiers((turret, gun));
        }

        // Durability: no longer fragile while its cover is open.
        if (TryComp<DeployableTurretComponent>(turret, out var deployable))
            _turrets.SetResilientWhenDeployed((turret, deployable));
    }

    private void OnTurretMapInit(Entity<StationAiTurretComponent> ent, ref MapInitEvent args)
    {
        // Newly built AI turrets inherit the upgrade.
        var query = EntityQueryEnumerator<MalfunctionAiComponent>();
        while (query.MoveNext(out _, out var malf))
        {
            if (!malf.TurretsUpgraded)
                continue;

            ApplyTurretUpgrade(ent.Owner, malf);
            return;
        }
    }

    private void OnCameraListen(Entity<SurveillanceCameraComponent> ent, ref ListenEvent args)
    {
        var camXform = Transform(ent.Owner);

        var query = EntityQueryEnumerator<MalfunctionAiComponent>();
        while (query.MoveNext(out var aiUid, out var malf))
        {
            if (!malf.CameraMicsUnlocked)
                continue;

            // Don't echo the AI's own speech back at it.
            if (args.Source == aiUid)
                continue;

            if (!TryComp<ActorComponent>(aiUid, out var actor))
                continue;

            // The AI's eye must actually be watching near this camera.
            if (!_stationAi.TryGetCore(aiUid, out var core) || core.Comp?.RemoteEntity is not { } eye)
                continue;

            var eyeXform = Transform(eye);
            if (eyeXform.MapID != camXform.MapID)
                continue;

            var distance = (_transform.GetWorldPosition(eyeXform) - _transform.GetWorldPosition(camXform)).Length();
            if (distance > malf.CameraMicEyeRange)
                continue;

            var wrapped = Loc.GetString("malfunction-ai-camera-mic-relay",
                ("speaker", Name(args.Source)),
                ("message", args.Message));

            _chatManager.ChatMessageToOne(ChatChannel.Local,
                args.Message,
                wrapped,
                ent.Owner,
                false,
                actor.PlayerSession.Channel);
        }
    }

    // --- Decrypt Syndicate keys ---

    private void OnDecryptSyndicateKeys(Entity<MalfunctionAiComponent> ent, ref MalfDecryptSyndicateKeysEvent args)
    {
        EnsureComp<ActiveRadioComponent>(ent.Owner).Channels.Add(ent.Comp.SyndicateRadioChannel);
        EnsureComp<IntrinsicRadioTransmitterComponent>(ent.Owner).Channels.Add(ent.Comp.SyndicateRadioChannel);

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-decrypt-success"), ent.Owner);
    }

    private void AnnounceFromAi(EntityUid ai, string message)
    {
        var station = _station.GetOwningStation(ai);
        if (station == null)
            return;

        _chat.DispatchStationAnnouncement(
            station.Value,
            message,
            Loc.GetString("malfunction-ai-announcement-sender"),
            playDefaultSound: true,
            colorOverride: Color.Red);
    }
}

/// <summary>
/// Raised broadcast when a Malfunction AI arms the Doomsday device.
/// The Malfunction AI game rule listens for this and starts the countdown / blast.
/// </summary>
[ByRefEvent]
public readonly record struct MalfDoomsdayArmedEvent(EntityUid Ai);
