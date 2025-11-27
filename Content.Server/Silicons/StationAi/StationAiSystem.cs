// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JustCone <141039037+JustCone14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 coolboy911 <85909253+coolboy911@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 saintmuntzer <47153094+saintmuntzer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Dreykor <Dreykor12@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 funkystationbot <funky@funkystation.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Construction;
using Content.Server.Destructible;
using Content.Server.Ghost;
using Content.Shared.Mind;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Spawners.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.Alert;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.Rejuvenate;
using Content.Shared.Roles;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Speech.Components;
using Content.Shared.StationAi;
using Content.Shared.Turrets;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using static Content.Server.Chat.Systems.ChatSystem;
using Robust.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Content.Shared.Chat;
using Robust.Shared.Audio;
using System.Linq;
using Content.Server._Funkystation.MalfAI;
using Content.Server._Funkystation.MalfAI.Components;

namespace Content.Server.Silicons.StationAi;

public sealed class StationAiSystem : SharedStationAiSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DestructibleSystem _destructible = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly MalfAiDoomsdaySystem _doomsday = default!;

    private readonly HashSet<Entity<StationAiCoreComponent>> _stationAiCores = new();

    private readonly ProtoId<ChatNotificationPrototype> _turretIsAttackingChatNotificationPrototype = "TurretIsAttacking";
    private readonly ProtoId<ChatNotificationPrototype> _aiWireSnippedChatNotificationPrototype = "AiWireSnipped";
    private readonly ProtoId<ChatNotificationPrototype> _aiLosingPowerChatNotificationPrototype = "AiLosingPower";
    private readonly ProtoId<ChatNotificationPrototype> _aiCriticalPowerChatNotificationPrototype = "AiCriticalPower";

    private readonly ProtoId<JobPrototype> _stationAiJob = "StationAi";
    private readonly EntProtoId _stationAiBrain = "StationAiBrain";

    private readonly ProtoId<AlertPrototype> _batteryAlert = "BorgBattery";
    private readonly ProtoId<AlertPrototype> _damageAlert = "BorgHealth";

    /// <summary>
    /// Tracks the last time each AI core was alerted about being under attack to implement cooldown.
    /// </summary>
    private readonly Dictionary<EntityUid, TimeSpan> _attackAlertCooldowns = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiCoreComponent, AfterConstructionChangeEntityEvent>(AfterConstructionChangeEntity);
        SubscribeLocalEvent<StationAiCoreComponent, ContainerSpawnEvent>(OnContainerSpawn);
        SubscribeLocalEvent<StationAiCoreComponent, ApcPowerReceiverBatteryChangedEvent>(OnApcBatteryChanged);
        SubscribeLocalEvent<StationAiCoreComponent, ChargeChangedEvent>(OnChargeChanged);
        SubscribeLocalEvent<StationAiCoreComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<StationAiCoreComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<StationAiCoreComponent, DoAfterAttemptEvent<IntellicardDoAfterEvent>>(OnDoAfterAttempt);
        SubscribeLocalEvent<StationAiCoreComponent, RejuvenateEvent>(OnRejuvenate);

        SubscribeLocalEvent<ExpandICChatRecipientsEvent>(OnExpandICChatRecipients);
        SubscribeLocalEvent<StationAiTurretComponent, AmmoShotEvent>(OnAmmoShot);
        SubscribeLocalEvent<ApcComponent, ComponentShutdown>(OnApcShutdown);
    }

    private void AfterConstructionChangeEntity(Entity<StationAiCoreComponent> ent, ref AfterConstructionChangeEntityEvent args)
    {
        if (!_container.TryGetContainer(ent, StationAiCoreComponent.BrainContainer, out var container) ||
            container.Count == 0)
        {
            return;
        }

        var brain = container.ContainedEntities[0];

        if (_mind.TryGetMind(brain, out var mindId, out var mind))
        {
            // Found an existing mind to transfer into the AI core
            var aiBrain = Spawn(_stationAiBrain, Transform(ent.Owner).Coordinates);
            _roles.MindAddJobRole(mindId, mind, false, _stationAiJob);
            _mind.TransferTo(mindId, aiBrain);

            if (!TryComp<StationAiHolderComponent>(ent, out var targetHolder) ||
                !_slots.TryInsert(ent, targetHolder.Slot, aiBrain, null))
            {
                QueueDel(aiBrain);
            }
        }

        // TODO: We should consider keeping the borg brain inside the AI core.
        // When the core is destroyed, the station AI can be transferred into the brain,
        // then dropped on the ground. The deceased AI can then be revived later,
        // instead of being lost forever.
        QueueDel(brain);
    }

    private void OnContainerSpawn(Entity<StationAiCoreComponent> ent, ref ContainerSpawnEvent args)
    {
        // Ensure that players that recently joined the round will spawn
        // into an AI core that has a full battery and full integrity.
        if (TryComp<BatteryComponent>(ent, out var battery))
        {
            _battery.SetCharge(ent, battery.MaxCharge);
        }

        if (TryComp<DamageableComponent>(ent, out var damageable))
        {
            _damageable.SetAllDamage(ent, damageable, 0);
        }
    }

    protected override void OnAiInsert(Entity<StationAiCoreComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        base.OnAiInsert(ent, ref args);

        UpdateBatteryAlert(ent);
        UpdateCoreIntegrityAlert(ent);
        UpdateDamagedAccent(ent);
    }

    protected override void OnAiRemove(Entity<StationAiCoreComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        base.OnAiRemove(ent, ref args);

        _alerts.ClearAlert(args.Entity, _batteryAlert);
        _alerts.ClearAlert(args.Entity, _damageAlert);

        if (TryComp<DamagedSiliconAccentComponent>(args.Entity, out var accent))
        {
            accent.OverrideChargeLevel = null;
            accent.OverrideTotalDamage = null;
            accent.DamageAtMaxCorruption = null;
        }
    }

    protected override void OnMobStateChanged(Entity<StationAiCustomizationComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive)
        {
            SetStationAiState(ent, StationAiState.Dead);
            return;
        }

        var state = StationAiState.Rebooting;

        if (_mind.TryGetMind(ent, out var _, out var mind) && !mind.IsVisitingEntity)
        {
            state = StationAiState.Occupied;
        }

        if (TryGetCore(ent, out var aiCore) && aiCore.Comp != null)
        {
            var aiCoreEnt = (aiCore.Owner, aiCore.Comp);

            if (SetupEye(aiCoreEnt))
                AttachEye(aiCoreEnt);
        }

        SetStationAiState(ent, state);
    }

    private void OnDestruction(Entity<StationAiCoreComponent> ent, ref DestructionEventArgs args)
    {
        var station = _station.GetOwningStation(ent);

        if (station == null)
            return;

        if (!HasComp<ContainerSpawnPointComponent>(ent))
            return;

        // If the destroyed core could act as a player spawn point,
        // reduce the number of available AI jobs by one
        _stationJobs.TryAdjustJobSlot(station.Value, _stationAiJob, -1, false, true);
    }

    private void OnApcBatteryChanged(Entity<StationAiCoreComponent> ent, ref ApcPowerReceiverBatteryChangedEvent args)
    {
        if (!args.Enabled)
            return;

        if (!TryGetHeld((ent.Owner, ent.Comp), out var held))
            return;

        var ev = new ChatNotificationEvent(_aiLosingPowerChatNotificationPrototype, ent);
        RaiseLocalEvent(held.Value, ref ev);
    }

    private void OnChargeChanged(Entity<StationAiCoreComponent> entity, ref ChargeChangedEvent args)
    {
        UpdateBatteryAlert(entity);
        UpdateDamagedAccent(entity);
    }

    private void OnDamageChanged(Entity<StationAiCoreComponent> entity, ref DamageChangedEvent args)
    {
        UpdateCoreIntegrityAlert(entity);
        UpdateDamagedAccent(entity);

        if (args.DamageDelta == null || args.DamageDelta.GetTotal() <= 0)
            return;

        var currentTime = _timing.CurTime;
        if (_attackAlertCooldowns.TryGetValue(entity.Owner, out var lastAlertTime))
        {
            if (currentTime - lastAlertTime < AttackAlertCooldown)
                return;
        }

        _attackAlertCooldowns[entity.Owner] = currentTime;

        // Try to get the AI entity held in this core
        if (!TryGetHeld(entity.AsNullable(), out var aiEntity)
            || !TryComp<ActorComponent>(aiEntity, out var actor))
            return;

        // Send alert message to the AI player
        var msg = Loc.GetString("ai-core-under-attack");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chat.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Red);

        // Play alert sound, could probably make a unique sound for this but for now, default notice noise
        if (_mind.TryGetMind(aiEntity.Value, out var mindId, out _))
        {
            var alertSound = new SoundPathSpecifier("/Audio/Misc/notice1.ogg");
            _roles.MindPlaySound(mindId, alertSound);
        }
    }

    private void UpdateDamagedAccent(Entity<StationAiCoreComponent> ent)
    {
        if (!TryGetHeld((ent.Owner, ent.Comp), out var held))
            return;

        if (!TryComp<DamagedSiliconAccentComponent>(held, out var accent))
            return;

        if (TryComp<BatteryComponent>(ent, out var battery))
            accent.OverrideChargeLevel = battery.CurrentCharge / battery.MaxCharge;

        if (TryComp<DamageableComponent>(ent, out var damageable))
            accent.OverrideTotalDamage = damageable.TotalDamage;

        if (TryComp<DestructibleComponent>(ent, out var destructible))
            accent.DamageAtMaxCorruption = _destructible.DestroyedAt(ent, destructible);

        Dirty(held.Value, accent);
    }

    private void UpdateBatteryAlert(Entity<StationAiCoreComponent> ent)
    {
        if (!TryComp<BatteryComponent>(ent, out var battery))
            return;

        if (!TryGetHeld((ent.Owner, ent.Comp), out var held))
            return;

        if (!_proto.TryIndex(_batteryAlert, out var proto))
            return;

        var chargePercent = battery.CurrentCharge / battery.MaxCharge;
        var chargeLevel = Math.Round(chargePercent * proto.MaxSeverity);

        _alerts.ShowAlert(held.Value, _batteryAlert, (short)Math.Clamp(chargeLevel, 0, proto.MaxSeverity));

        if (TryComp<ApcPowerReceiverBatteryComponent>(ent, out var apcBattery) &&
            apcBattery.Enabled &&
            chargePercent < 0.2)
        {
            var ev = new ChatNotificationEvent(_aiCriticalPowerChatNotificationPrototype, ent);
            RaiseLocalEvent(held.Value, ref ev);
        }
    }

    private void UpdateCoreIntegrityAlert(Entity<StationAiCoreComponent> ent)
    {
        if (!TryComp<DamageableComponent>(ent, out var damageable))
            return;

        if (!TryComp<DestructibleComponent>(ent, out var destructible))
            return;

        if (!TryGetHeld((ent.Owner, ent.Comp), out var held))
            return;

        if (!_proto.TryIndex(_damageAlert, out var proto))
            return;

        var damagePercent = damageable.TotalDamage / _destructible.DestroyedAt(ent, destructible);
        var damageLevel = Math.Round(damagePercent.Float() * proto.MaxSeverity);

        _alerts.ShowAlert(held.Value, _damageAlert, (short)Math.Clamp(damageLevel, 0, proto.MaxSeverity));
    }

    private void OnDoAfterAttempt(Entity<StationAiCoreComponent> ent, ref DoAfterAttemptEvent<IntellicardDoAfterEvent> args)
    {
        if (TryGetHeld((ent.Owner, ent.Comp), out _))
            return;

        // Prevent AIs from being uploaded into an unpowered or broken AI core.

        if (TryComp<ApcPowerReceiverComponent>(ent, out var apcPower) && !apcPower.Powered)
        {
            _popups.PopupEntity(Loc.GetString("station-ai-has-no-power-for-upload"), ent, args.Event.User);
            args.Cancel();
        }
        else if (TryComp<DestructibleComponent>(ent, out var destructible) && destructible.IsBroken)
        {
            _popups.PopupEntity(Loc.GetString("station-ai-is-too-damaged-for-upload"), ent, args.Event.User);
            args.Cancel();
        }
    }

    public override void KillHeldAi(Entity<StationAiCoreComponent> ent)
    {
        base.KillHeldAi(ent);

        if (TryGetHeld((ent.Owner, ent.Comp), out var held))
        {
            if (_mind.TryGetMind(held.Value, out var mindId, out var mind))
            {
                _ghost.OnGhostAttempt(mindId, canReturnGlobal: true, mind: mind);
                RemComp<StationAiOverlayComponent>(held.Value);
            }

            // Funkystation -> Malf Ai
            if (TryComp<MalfAiDoomsdayComponent>(held, out var doomsday)
                && doomsday.Active)
                _doomsday.AbortDoomsday(held.Value, doomsday, "malfai-doomsday-abort-dead");
        }

        ClearEye(ent);
    }

    private void OnRejuvenate(Entity<StationAiCoreComponent> ent, ref RejuvenateEvent args)
    {
        if (TryGetHeld((ent.Owner, ent.Comp), out var held))
        {
            _mobState.ChangeMobState(held.Value, MobState.Alive);
            EnsureComp<StationAiOverlayComponent>(held.Value);
        }

        if (TryComp<StationAiHolderComponent>(ent, out var holder))
        {
            _appearance.SetData(ent, StationAiVisuals.Broken, false);
            UpdateAppearance((ent, holder));
        }
    }

    private void OnExpandICChatRecipients(ExpandICChatRecipientsEvent ev)
    {
        var xformQuery = GetEntityQuery<TransformComponent>();
        var sourceXform = Transform(ev.Source);
        var sourcePos = _xforms.GetWorldPosition(sourceXform, xformQuery);

        // This function ensures that chat popups appear on camera views that have connected microphones.
        var query = EntityQueryEnumerator<StationAiCoreComponent, TransformComponent>();
        while (query.MoveNext(out var ent, out var entStationAiCore, out var entXform))
        {
            var stationAiCore = new Entity<StationAiCoreComponent?>(ent, entStationAiCore);

            if (!TryGetHeld(stationAiCore, out var insertedAi) || !TryComp(insertedAi, out ActorComponent? actor))
                continue;

            if (stationAiCore.Comp?.RemoteEntity == null || stationAiCore.Comp.Remote)
                continue;

            var xform = Transform(stationAiCore.Comp.RemoteEntity.Value);

            var range = (xform.MapID != sourceXform.MapID)
                ? -1
                : (sourcePos - _xforms.GetWorldPosition(xform, xformQuery)).Length();

            if (range < 0 || range > ev.VoiceRange)
                continue;

            ev.Recipients.TryAdd(actor.PlayerSession, new ICChatRecipientData(range, false));
        }
    }

    private void OnAmmoShot(Entity<StationAiTurretComponent> ent, ref AmmoShotEvent args)
    {
        var xform = Transform(ent);

        if (!TryComp(xform.GridUid, out MapGridComponent? grid))
            return;

        var ais = GetStationAIs(xform.GridUid.Value);

        foreach (var ai in ais)
        {
            var ev = new ChatNotificationEvent(_turretIsAttackingChatNotificationPrototype, ent);

            if (TryComp<DeviceNetworkComponent>(ent, out var deviceNetwork))
                ev.SourceNameOverride = Loc.GetString("station-ai-turret-component-name", ("name", Name(ent)), ("address", deviceNetwork.Address));

            RaiseLocalEvent(ai, ref ev);
        }
    }

    public override bool SetVisionEnabled(Entity<StationAiVisionComponent> entity, bool enabled, bool announce = false)
    {
        if (!base.SetVisionEnabled(entity, enabled, announce))
            return false;

        if (announce)
            AnnounceSnip(entity.Owner);

        return true;
    }

    public override bool SetWhitelistEnabled(Entity<StationAiWhitelistComponent> entity, bool enabled, bool announce = false)
    {
        if (!base.SetWhitelistEnabled(entity, enabled, announce))
            return false;

        if (announce)
            AnnounceSnip(entity.Owner);

        return true;
    }

    private void AnnounceSnip(EntityUid uid)
    {
        var xform = Transform(uid);

        if (!TryComp(xform.GridUid, out MapGridComponent? grid))
            return;

        var ais = GetStationAIs(xform.GridUid.Value);

        foreach (var ai in ais)
        {
            if (!StationAiCanDetectWireSnipping(ai))
                continue;

            var ev = new ChatNotificationEvent(_aiWireSnippedChatNotificationPrototype, uid);

            var tile = Maps.LocalToTile(xform.GridUid.Value, grid, xform.Coordinates);
            ev.SourceNameOverride = tile.ToString();

            RaiseLocalEvent(ai, ref ev);
        }
    }

    private bool StationAiCanDetectWireSnipping(EntityUid uid)
    {
        // TODO: The ability to detect snipped AI interaction wires
        // should be a MALF ability and/or a purchased upgrade rather
        // than something available to the station AI by default.
        // When these systems are added, add the appropriate checks here.

        return false;
    }

    public HashSet<EntityUid> GetStationAIs(EntityUid gridUid)
    {
        _stationAiCores.Clear();
        _lookup.GetChildEntities(gridUid, _stationAiCores);

        var hashSet = new HashSet<EntityUid>();

        foreach (var stationAiCore in _stationAiCores)
        {
            if (!TryGetHeld((stationAiCore, stationAiCore.Comp), out var insertedAi))
                continue;

            hashSet.Add(insertedAi.Value);
        }

        return hashSet;
    }

    /// <summary>
    /// Funky edit, AI gets alert for damage
    /// </summary>
    private static readonly TimeSpan AttackAlertCooldown = TimeSpan.FromSeconds(10);

    private void OnAiCoreDamaged(EntityUid uid, StationAiCoreComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null || args.DamageDelta.GetTotal() <= 0)
            return;

        var currentTime = _timing.CurTime;
        if (_attackAlertCooldowns.TryGetValue(uid, out var lastAlertTime))
        {
            if (currentTime - lastAlertTime < AttackAlertCooldown)
                return;
        }

        _attackAlertCooldowns[uid] = currentTime;

        // Try to get the AI entity held in this core
        var aiCore = new Entity<StationAiCoreComponent?>(uid, component);
        if (!TryGetHeld(aiCore, out var aiEntity)
            || !TryComp(aiEntity, out ActorComponent? actor))
            return;

        // Send alert message to the AI player
        var msg = Loc.GetString("ai-core-under-attack");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chat.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Red);

        // Play alert sound, could probably make a unique sound for this but for now, default notice noise
        if (_mind.TryGetMind(aiEntity.Value, out var mindId, out _))
        {
            var alertSound = new SoundPathSpecifier("/Audio/Misc/notice1.ogg");
            _roles.MindPlaySound(mindId, alertSound);
        }
    }

    // Funky edit, malf brain destroyed on APC destruction
    private void OnApcShutdown(EntityUid uid, ApcComponent component, ComponentShutdown args)
    {
        DestroyAiBrainInContainer(uid, StationAiHolderComponent.Container);
    }


    private void DestroyAiBrainInContainer(EntityUid parentEntity, BaseContainer? container)
    {
        if (container == null)
            return;

        foreach (var containedEntity in container.ContainedEntities)
        {
            if (HasComp<StationAiHeldComponent>(containedEntity))
            {
                // Make station announcement about AI destruction
                var msg = Loc.GetString("ai-destroyed-announcement");
                _chatSystem.DispatchStationAnnouncement(parentEntity, msg, playDefaultSound: true);

                // Delete the AI brain
                QueueDel(containedEntity);
            }
        }
    }

    private void DestroyAiBrainInContainer(EntityUid parentEntity, string containerName)
    {
        if (!_containers.TryGetContainer(parentEntity, containerName, out var container))
            return;

        DestroyAiBrainInContainer(parentEntity, container);
    }
    // End funky edit
}
