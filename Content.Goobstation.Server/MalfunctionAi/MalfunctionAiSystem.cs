// SPDX-FileCopyrightText: 2026 Jonikibaka
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
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
using Content.Goobstation.Shared.Overlays;
using Content.Shared.Actions;
using Content.Shared.Alert;
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
using Content.Shared.Emp;
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
        SubscribeLocalEvent<SurveillanceCameraComponent, EmpAttemptEvent>(OnCameraEmpAttempt);
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

        // Ability tuning lives in focused per-ability components rather than one god-component.
        // Button abilities have their tuning present from the start (the button itself is still
        // gated by the store purchase); passive upgrades add their component only when bought.
        EnsureComp<MalfOverloadComponent>(ent.Owner);
        EnsureComp<MalfRcdComponent>(ent.Owner);
        EnsureComp<MalfLockdownComponent>(ent.Owner);
        EnsureComp<MalfGyroscopeComponent>(ent.Owner);
        EnsureComp<MalfShuntComponent>(ent.Owner);
        EnsureComp<MalfHackCyborgComponent>(ent.Owner);

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
        }

        // End active lockdowns once their timer runs out.
        var lockdownQuery = EntityQueryEnumerator<MalfLockdownComponent>();
        while (lockdownQuery.MoveNext(out var lockUid, out var lockdown))
        {
            if (lockdown.EndTime != null && now >= lockdown.EndTime)
                EndLockdown((lockUid, lockdown));
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
