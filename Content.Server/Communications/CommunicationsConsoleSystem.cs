using Content.Server.Access.Systems;
using Content.Server.Administration.Logs;
using Content.Goobstation.Shared.AlertLevel;
using Content.Server.AlertLevel;
using Content.Server.Chat.Systems;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Popups;
using Content.Server.RoundEnd;
using Content.Server.Screens.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Communications;
using Content.Shared.Database;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Server.Communications
{
    public sealed class CommunicationsConsoleSystem : EntitySystem
    {
        [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
        [Dependency] private readonly AlertLevelSystem _alertLevelSystem = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
        [Dependency] private readonly EmagSystem _emag = default!;
        [Dependency] private readonly EmergencyShuttleSystem _emergency = default!;
        [Dependency] private readonly IdCardSystem _idCardSystem = default!;
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
            SubscribeLocalEvent<RoundEndSystemChangedEvent>(_ => OnGenericBroadcastEvent());
            SubscribeLocalEvent<AlertLevelDelayFinishedEvent>(_ => OnGenericBroadcastEvent());

            SubscribeLocalEvent<AlertLevelSelectAttemptEvent>(OnAlertLevelSelectAttempt);

            // Messages from the BUI
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleSelectAlertLevelMessage>(OnSelectAlertLevelMessage);
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleAnnounceMessage>(OnAnnounceMessage);
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleBroadcastMessage>(OnBroadcastMessage);
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleCallEmergencyShuttleMessage>(OnCallShuttleMessage);
            SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleRecallEmergencyShuttleMessage>(OnRecallShuttleMessage);

            // On console init, set cooldown
            SubscribeLocalEvent<CommunicationsConsoleComponent, MapInitEvent>(OnCommunicationsConsoleMapInit);

            SubscribeLocalEvent<CommunicationsConsoleComponent, GotEmaggedEvent>(OnEmagged); // Goobstation
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
            UpdateCommsConsoleInterface(uid, comp);
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
                if (entStation == null && HasComp<StationAiHeldComponent>(uid))
                {
                    var parent = Transform(uid).ParentUid;
                    if (parent.IsValid())
                        entStation = _stationSystem.GetOwningStation(parent);
                }

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
            if (stationUid == null && HasComp<StationAiHeldComponent>(uid))
            {
                var parent = Transform(uid).ParentUid;
                if (parent.IsValid())
                    stationUid = _stationSystem.GetOwningStation(parent);
            }

            List<string>? levels = null;
            string currentLevel = default!;
            float currentDelay = 0;

            if (stationUid != null)
            {
                if (TryComp(stationUid.Value, out AlertLevelComponent? alertComp) &&
                    alertComp.AlertLevels != null)
                {
                    currentLevel = alertComp.CurrentLevel;
                    currentDelay = _alertLevelSystem.GetAlertLevelDelay(stationUid.Value, alertComp);

                    if (alertComp.IsSelectable)
                    {
                        // If this console belongs to a silicon (AI or borg), and current level is Cyan,
                        // silicon authority is revoked: no alert levels can be selected.
                        if (IsSilicon(uid) && currentLevel.Equals("cyan", StringComparison.OrdinalIgnoreCase))
                        {
                            levels = null;
                        }
                        else
                        {
                            levels = new();
                            foreach (var (id, detail) in alertComp.AlertLevels.Levels)
                            {
                                if (!detail.Selectable)
                                    continue;

                                // Silicons (AI and borgs) cannot set the alert level to Cyan.
                                if (IsSilicon(uid) && id.Equals("cyan", StringComparison.OrdinalIgnoreCase))
                                    continue;

                                levels.Add(id);
                            }
                        }
                    }
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

        private static readonly ProtoId<AccessLevelPrototype> CommandAccess = "Command";
        private static readonly ProtoId<AccessLevelPrototype> CaptainAccess = "Captain";
        private static readonly ProtoId<AccessLevelPrototype> HosAccess = "HeadOfSecurity";
        private static readonly ProtoId<AccessLevelPrototype> HopAccess = "HeadOfPersonnel";
        private static readonly ProtoId<AccessLevelPrototype> RdAccess = "ResearchDirector";
        private static readonly ProtoId<AccessLevelPrototype> CeAccess = "ChiefEngineer";
        private static readonly ProtoId<AccessLevelPrototype> CmoAccess = "ChiefMedicalOfficer";
        private static readonly ProtoId<AccessLevelPrototype> CentcomAccess = "CentralCommand";

        private bool IsAi(EntityUid entity)
        {
            return HasComp<StationAiCoreComponent>(entity) ||
                   HasComp<StationAiHeldComponent>(entity) ||
                   HasComp<StationAiEyeComponent>(entity);
        }

        private bool IsBorg(EntityUid entity)
        {
            return HasComp<BorgChassisComponent>(entity) ||
                   HasComp<BorgBrainComponent>(entity);
        }

        private bool IsSilicon(EntityUid entity)
        {
            return IsAi(entity) || IsBorg(entity);
        }

        private bool IsCommand(EntityUid user)
        {
            var tags = _accessReaderSystem.FindAccessTags(user);
            if (tags.Contains(CommandAccess) ||
                tags.Contains(CaptainAccess) ||
                tags.Contains(HosAccess) ||
                tags.Contains(HopAccess) ||
                tags.Contains(RdAccess) ||
                tags.Contains(CeAccess) ||
                tags.Contains(CmoAccess) ||
                tags.Contains(CentcomAccess))
            {
                return true;
            }

            if (_idCardSystem.TryFindIdCard(user, out var idCard))
            {
                var cardTags = _accessReaderSystem.FindAccessTags(idCard);
                if (cardTags.Contains(CommandAccess) ||
                    cardTags.Contains(CaptainAccess) ||
                    cardTags.Contains(HosAccess) ||
                    cardTags.Contains(HopAccess) ||
                    cardTags.Contains(RdAccess) ||
                    cardTags.Contains(CeAccess) ||
                    cardTags.Contains(CmoAccess) ||
                    cardTags.Contains(CentcomAccess))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnAlertLevelSelectAttempt(ref AlertLevelSelectAttemptEvent ev)
        {
            if (ev.Cancelled)
                return;

            if (TryComp<AlertLevelComponent>(ev.Station, out var alertComp))
            {
                // If Cyan is called, AI's authority is revoked, in other words AI shouldn't be able to change alert level
                if (alertComp.CurrentLevel.Equals("cyan", StringComparison.OrdinalIgnoreCase) && IsSilicon(ev.User))
                {
                    _popupSystem.PopupEntity(Loc.GetString("alert-level-cyan-revoked-authority"), ev.Console, ev.User, PopupType.MediumCaution);
                    ev.Cancelled = true;
                    return;
                }
            }

            // Ensure only command can set Cyan, AI and borgs should NOT be able to set the alert level to cyan.
            if (ev.Level.Equals("cyan", StringComparison.OrdinalIgnoreCase))
            {
                if (IsSilicon(ev.User) || !IsCommand(ev.User))
                {
                    _popupSystem.PopupEntity(Loc.GetString("alert-level-cyan-command-only"), ev.Console, ev.User, PopupType.MediumCaution);
                    ev.Cancelled = true;
                }
            }
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
            if (stationUid == null && HasComp<StationAiHeldComponent>(uid))
            {
                var parent = Transform(uid).ParentUid;
                if (parent.IsValid())
                    stationUid = _stationSystem.GetOwningStation(parent);
            }

            if (stationUid == null)
            {
                stationUid = _stationSystem.GetOwningStation(mob);
                if (stationUid == null && HasComp<StationAiHeldComponent>(mob))
                {
                    var parent = Transform(mob).ParentUid;
                    if (parent.IsValid())
                        stationUid = _stationSystem.GetOwningStation(parent);
                }
            }

            if (stationUid != null)
            {
                if (TryComp<AlertLevelComponent>(stationUid.Value, out var alertComp))
                {
                    // If Cyan is called, AI's authority is revoked, in other words AI shouldn't be able to change alert level
                    if (alertComp.CurrentLevel.Equals("cyan", StringComparison.OrdinalIgnoreCase) && IsSilicon(mob))
                    {
                        _popupSystem.PopupCursor(Loc.GetString("alert-level-cyan-revoked-authority"), message.Actor, PopupType.MediumCaution);
                        return;
                    }
                }

                // Ensure only command can set Cyan, AI and borgs should NOT be able to set the alert level to cyan.
                if (message.Level.Equals("cyan", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsSilicon(mob) || !IsCommand(mob))
                    {
                        _popupSystem.PopupCursor(Loc.GetString("alert-level-cyan-command-only"), message.Actor, PopupType.MediumCaution);
                        return;
                    }
                }

                // Goobstation - allow systems (e.g. amber alert gating) to veto the selection.
                var attempt = new AlertLevelSelectAttemptEvent(stationUid.Value, uid, mob, message.Level);
                RaiseLocalEvent(ref attempt);
                if (attempt.Cancelled)
                    return;

                _alertLevelSystem.SetLevel(stationUid.Value, message.Level, true, true);
                // Goob
                _adminLogger.Add(LogType.Chat,
                                 LogImpact.Medium,
                                 $"{ToPrettyString(message.Actor):actor} set alert level to {message.Level:level} with {ToPrettyString(uid):subject}");
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

            if (comp.AnnounceSentBy)
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
            _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(message.Actor):player} has recalled the shuttle.");
        }

        // Goobstation start
        private void OnEmagged(Entity<CommunicationsConsoleComponent> ent, ref GotEmaggedEvent args)
        {
            if (args.Handled)
                return;

            var (uid, comp) = ent;
            args.Repeatable = true;

            if (_emag.CompareFlag(args.Type, EmagType.Access))
            {
                var amberStation = _stationSystem.GetOwningStation(uid);
                if (amberStation != null
                    && TryComp<AmberAlertComponent>(amberStation, out var amber)
                    && !amber.Unlocked)
                {
                    amber.Unlocked = true;
                    _popupSystem.PopupEntity(Loc.GetString("alert-level-amber-unlocked"), uid, args.UserUid, PopupType.Medium);
                    args.Handled = true;
                }
            }

            if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
                return;

            var stationUid = _stationSystem.GetOwningStation(uid);
            if (stationUid != null)
                _alertLevelSystem.SetLevel(stationUid.Value, comp.AlertLevelOnEmag, true, true);

            args.Handled = true;
        }
        // Goobstation end
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
