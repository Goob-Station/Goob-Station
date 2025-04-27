// SPDX-FileCopyrightText: 2022 Júlio César Ueti <52474532+Mirino97@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Morber <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <deltanedas@laptop>
// SPDX-FileCopyrightText: 2023 deltanedas <user@zenith>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JustCone <141039037+JustCone14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Justin Pfeifler <jrpl101998@gmail.com>
// SPDX-FileCopyrightText: 2024 Kot <1192090+koteq@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 Robert V <vincerob@oregonstate.edu>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 avery <51971268+graevy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 coolboy911 <85909253+coolboy911@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2024 saintmuntzer <47153094+saintmuntzer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 superjj18 <gagnonjake@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.AlertLevel;
using Content.Server.Chat.Systems;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Popups;
using Content.Server.RoundEnd;
using Content.Server.Screens.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Communications;
using Content.Shared.Database;
using Content.Shared.DeviceNetwork;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;

namespace Content.Server.Communications
{
    public sealed class CommunicationsConsoleSystem : EntitySystem
    {
        [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
        [Dependency] private readonly AlertLevelSystem _alertLevelSystem = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
        [Dependency] private readonly EmergencyShuttleSystem _emergency = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
        [Dependency] private readonly StationSystem _stationSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;

        private const float UIUpdateInterval = 5.0f;

        public override void Initialize()
        {
            // All events that refresh the BUI
            SubscribeLocalEvent<AlertLevelChangedEvent>(OnAlertLevelChanged);
            SubscribeLocalEvent<CommunicationsConsoleComponent, ComponentInit>((uid, comp, _) => UpdateCommsConsoleInterface(uid, comp));
            SubscribeLocalEvent<RoundEndSystemChangedEvent>(_ => OnGenericBroadcastEvent());
            SubscribeLocalEvent<AlertLevelDelayFinishedEvent>(_ => OnGenericBroadcastEvent());

            // Messages from the BUI
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleSelectAlertLevelMessage>(OnSelectAlertLevelMessage);
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleAnnounceMessage>(OnAnnounceMessage);
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleBroadcastMessage>(OnBroadcastMessage);
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleCallEmergencyShuttleMessage>(OnCallShuttleMessage);
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleRecallEmergencyShuttleMessage>(OnRecallShuttleMessage);

            // On console init, set cooldown
            SubscribeLocalEvent<CommunicationsConsoleComponent, MapInitEvent>(OnCommunicationsConsoleMapInit);
        }

        public override void Update(float frameTime)
        {
            var query = EntityQueryEnumerator<CommunicationsConsoleComponent>();
            while (query.MoveNext(out var uid, out var comp))
            {
                // TODO refresh the UI in a less horrible way
                if (comp.AnnouncementCooldownRemaining >= 0f)
                {
                    comp.AnnouncementCooldownRemaining -= frameTime;
                }

                comp.UIUpdateAccumulator += frameTime;

                if (comp.UIUpdateAccumulator < UIUpdateInterval)
                    continue;

                comp.UIUpdateAccumulator -= UIUpdateInterval;

                if (_uiSystem.IsUiOpen(uid, CommunicationsConsoleUiKey.Key))
                    UpdateCommsConsoleInterface(uid, comp);
            }

            base.Update(frameTime);
        }

        public void OnCommunicationsConsoleMapInit(EntityUid uid, CommunicationsConsoleComponent comp, MapInitEvent args)
        {
            comp.AnnouncementCooldownRemaining = comp.InitialDelay;
        }

        /// <summary>
        /// Update the UI of every comms console.
        /// </summary>
        private void OnGenericBroadcastEvent()
        {
            var query = EntityQueryEnumerator<CommunicationsConsoleComponent>();
            while (query.MoveNext(out var uid, out var comp))
            {
                UpdateCommsConsoleInterface(uid, comp);
            }
        }

        /// <summary>
        /// Updates all comms consoles belonging to the station that the alert level was set on
        /// </summary>
        /// <param name="args">Alert level changed event arguments</param>
        private void OnAlertLevelChanged(AlertLevelChangedEvent args)
        {
            var query = EntityQueryEnumerator<CommunicationsConsoleComponent>();
            while (query.MoveNext(out var uid, out var comp))
            {
                var entStation = _stationSystem.GetOwningStation(uid);
                if (args.Station == entStation)
                    UpdateCommsConsoleInterface(uid, comp);
            }
        }

        /// <summary>
        /// Updates the UI for all comms consoles.
        /// </summary>
        public void UpdateCommsConsoleInterface()
        {
            var query = EntityQueryEnumerator<CommunicationsConsoleComponent>();
            while (query.MoveNext(out var uid, out var comp))
            {
                UpdateCommsConsoleInterface(uid, comp);
            }
        }

        /// <summary>
        /// Updates the UI for a particular comms console.
        /// </summary>
        public void UpdateCommsConsoleInterface(EntityUid uid, CommunicationsConsoleComponent comp)
        {
            var stationUid = _stationSystem.GetOwningStation(uid);
            List<string>? levels = null;
            string currentLevel = default!;
            float currentDelay = 0;

            if (stationUid != null)
            {
                if (TryComp(stationUid.Value, out AlertLevelComponent? alertComp) &&
                    alertComp.AlertLevels != null)
                {
                    if (alertComp.IsSelectable)
                    {
                        levels = new();
                        foreach (var (id, detail) in alertComp.AlertLevels.Levels)
                        {
                            if (detail.Selectable)
                            {
                                levels.Add(id);
                            }
                        }
                    }

                    currentLevel = alertComp.CurrentLevel;
                    currentDelay = _alertLevelSystem.GetAlertLevelDelay(stationUid.Value, alertComp);
                }
            }

            _uiSystem.SetUiState(uid, CommunicationsConsoleUiKey.Key, new CommunicationsConsoleInterfaceState(
                CanAnnounce(comp),
                CanCallOrRecall(comp),
                levels,
                currentLevel,
                currentDelay,
                _roundEndSystem.ExpectedCountdownEnd
            ));
        }

        private static bool CanAnnounce(CommunicationsConsoleComponent comp)
        {
            return comp.AnnouncementCooldownRemaining <= 0f;
        }

        private bool CanUse(EntityUid user, EntityUid console)
        {
            if (TryComp<AccessReaderComponent>(console, out var accessReaderComponent))
            {
                return _accessReaderSystem.IsAllowed(user, console, accessReaderComponent);
            }
            return true;
        }

        private bool CanCallOrRecall(CommunicationsConsoleComponent comp)
        {
            // Defer to what the round end system thinks we should be able to do.
            if (_emergency.EmergencyShuttleArrived || !_roundEndSystem.CanCallOrRecall())
                return false;

            // Ensure that we can communicate with the shuttle (either call or recall)
            if (!comp.CanShuttle)
                return false;

            // Calling shuttle checks
            if (_roundEndSystem.ExpectedCountdownEnd is null)
                return true;

            // Recalling shuttle checks
            var recallThreshold = _cfg.GetCVar(CCVars.EmergencyRecallTurningPoint);

            // shouldn't really be happening if we got here
            if (_roundEndSystem.ShuttleTimeLeft is not { } left
                || _roundEndSystem.ExpectedShuttleLength is not { } expected)
                return false;

            return !(left.TotalSeconds / expected.TotalSeconds < recallThreshold);
        }

        private void OnSelectAlertLevelMessage(EntityUid uid, CommunicationsConsoleComponent comp, CommunicationsConsoleSelectAlertLevelMessage message)
        {
            if (message.Actor is not { Valid: true } mob)
                return;

            if (!CanUse(mob, uid))
            {
                _popupSystem.PopupCursor(Loc.GetString("comms-console-permission-denied"), message.Actor, PopupType.Medium);
                return;
            }

            var stationUid = _stationSystem.GetOwningStation(uid);
            if (stationUid != null)
            {
                _alertLevelSystem.SetLevel(stationUid.Value, message.Level, true, true);
            }
        }

        private void OnAnnounceMessage(EntityUid uid, CommunicationsConsoleComponent comp,
            CommunicationsConsoleAnnounceMessage message)
        {
            var maxLength = _cfg.GetCVar(CCVars.ChatMaxAnnouncementLength);
            var msg = SharedChatSystem.SanitizeAnnouncement(message.Message, maxLength);
            var author = Loc.GetString("comms-console-announcement-unknown-sender");
            if (message.Actor is { Valid: true } mob)
            {
                if (!CanAnnounce(comp))
                {
                    return;
                }

                if (!CanUse(mob, uid))
                {
                    _popupSystem.PopupEntity(Loc.GetString("comms-console-permission-denied"), uid, message.Actor);
                    return;
                }

                var tryGetIdentityShortInfoEvent = new TryGetIdentityShortInfoEvent(uid, mob);
                RaiseLocalEvent(tryGetIdentityShortInfoEvent);
                author = tryGetIdentityShortInfoEvent.Title;
            }

            comp.AnnouncementCooldownRemaining = comp.Delay;
            UpdateCommsConsoleInterface(uid, comp);

            var ev = new CommunicationConsoleAnnouncementEvent(uid, comp, msg, message.Actor);
            RaiseLocalEvent(ref ev);

            // allow admemes with vv
            Loc.TryGetString(comp.Title, out var title);
            title ??= comp.Title;

            msg += "\n" + Loc.GetString("comms-console-announcement-sent-by") + " " + author;
            if (comp.Global)
            {
                _chatSystem.DispatchGlobalAnnouncement(msg, title, announcementSound: comp.Sound, colorOverride: comp.Color);

                _adminLogger.Add(LogType.Chat, LogImpact.Low, $"{ToPrettyString(message.Actor):player} has sent the following global announcement: {msg}");
                return;
            }

            _chatSystem.DispatchStationAnnouncement(uid, msg, title, colorOverride: comp.Color);

            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"{ToPrettyString(message.Actor):player} has sent the following station announcement: {msg}");

        }

        private void OnBroadcastMessage(EntityUid uid, CommunicationsConsoleComponent component, CommunicationsConsoleBroadcastMessage message)
        {
            if (!TryComp<DeviceNetworkComponent>(uid, out var net))
                return;

            var payload = new NetworkPayload
            {
                [ScreenMasks.Text] = message.Message
            };

            _deviceNetworkSystem.QueuePacket(uid, null, payload, net.TransmitFrequency);

            _adminLogger.Add(LogType.DeviceNetwork, LogImpact.Low, $"{ToPrettyString(message.Actor):player} has sent the following broadcast: {message.Message:msg}");
        }

        private void OnCallShuttleMessage(EntityUid uid, CommunicationsConsoleComponent comp, CommunicationsConsoleCallEmergencyShuttleMessage message)
        {
            if (!CanCallOrRecall(comp))
                return;

            var mob = message.Actor;

            if (!CanUse(mob, uid))
            {
                _popupSystem.PopupEntity(Loc.GetString("comms-console-permission-denied"), uid, message.Actor);
                return;
            }

            var ev = new CommunicationConsoleCallShuttleAttemptEvent(uid, comp, mob);
            RaiseLocalEvent(ref ev);
            if (ev.Cancelled)
            {
                _popupSystem.PopupEntity(ev.Reason ?? Loc.GetString("comms-console-shuttle-unavailable"), uid, message.Actor);
                return;
            }

            _roundEndSystem.RequestRoundEnd(uid);
            _adminLogger.Add(LogType.Action, LogImpact.Extreme, $"{ToPrettyString(mob):player} has called the shuttle.");
        }

        private void OnRecallShuttleMessage(EntityUid uid, CommunicationsConsoleComponent comp, CommunicationsConsoleRecallEmergencyShuttleMessage message)
        {
            if (!CanCallOrRecall(comp))
                return;

            if (!CanUse(message.Actor, uid))
            {
                _popupSystem.PopupEntity(Loc.GetString("comms-console-permission-denied"), uid, message.Actor);
                return;
            }

            _roundEndSystem.CancelRoundEndCountdown(uid);
            _adminLogger.Add(LogType.Action, LogImpact.Extreme, $"{ToPrettyString(message.Actor):player} has recalled the shuttle.");
        }
    }

    /// <summary>
    /// Raised on announcement
    /// </summary>
    [ByRefEvent]
    public record struct CommunicationConsoleAnnouncementEvent(EntityUid Uid, CommunicationsConsoleComponent Component, string Text, EntityUid? Sender)
    {
        public EntityUid Uid = Uid;
        public CommunicationsConsoleComponent Component = Component;
        public EntityUid? Sender = Sender;
        public string Text = Text;
    }

    /// <summary>
    /// Raised on shuttle call attempt. Can be cancelled
    /// </summary>
    [ByRefEvent]
    public record struct CommunicationConsoleCallShuttleAttemptEvent(EntityUid Uid, CommunicationsConsoleComponent Component, EntityUid? Sender)
    {
        public bool Cancelled = false;
        public EntityUid Uid = Uid;
        public CommunicationsConsoleComponent Component = Component;
        public EntityUid? Sender = Sender;
        public string? Reason;
    }
}