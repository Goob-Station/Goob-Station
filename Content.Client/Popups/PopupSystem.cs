// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jack Fox <35575261+DubiousDoggo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 RedFoxIV <38788538+RedFoxIV@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Cooper Wallace <6856074+CooperWallace@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Cooper Wallace <CooperWallace@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Containers;
using Content.Client.UserInterface.Systems.Chat;
using Content.Goobstation.Common.CCVar; // Goobstation Change
using Content.Shared.Chat;
using Content.Shared.Examine;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Collections;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Replays;
using Robust.Shared.Timing;

namespace Content.Client.Popups
{
    public sealed class PopupSystem : SharedPopupSystem
    {
        [Dependency] private readonly IConfigurationManager _configManager = default!;
        [Dependency] private readonly IInputManager _inputManager = default!;
        [Dependency] private readonly IOverlayManager _overlay = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IPrototypeManager _prototype = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IReplayRecordingManager _replayRecording = default!;
        [Dependency] private readonly ExamineSystemShared _examine = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;

        public IReadOnlyCollection<WorldPopupLabel> WorldLabels => _aliveWorldLabels.Values;
        public IReadOnlyCollection<CursorPopupLabel> CursorLabels => _aliveCursorLabels.Values;

        private readonly Dictionary<WorldPopupData, WorldPopupLabel> _aliveWorldLabels = new();
        private readonly Dictionary<CursorPopupData, CursorPopupLabel> _aliveCursorLabels = new();

        public const float MinimumPopupLifetime = 0.7f;
        public const float MaximumPopupLifetime = 5f;
        public const float PopupLifetimePerCharacter = 0.04f;

        // WD EDIT START
        private static readonly Dictionary<PopupType, string> FontSizeDict = new()
        {
            { PopupType.Medium, "12" },
            { PopupType.MediumCaution, "12" },
            { PopupType.Large, "15" },
            { PopupType.LargeCaution, "15" }
        };

        private bool _shouldLogInChat;
        // WD EDIT END

        public override void Initialize()
        {
            SubscribeNetworkEvent<PopupCursorEvent>(OnPopupCursorEvent);
            SubscribeNetworkEvent<PopupCoordinatesEvent>(OnPopupCoordinatesEvent);
            SubscribeNetworkEvent<PopupEntityEvent>(OnPopupEntityEvent);
            SubscribeNetworkEvent<RoundRestartCleanupEvent>(OnRoundRestart);
            _overlay
                .AddOverlay(new PopupOverlay(
                    _configManager,
                    EntityManager,
                    _playerManager,
                    _prototype,
                    _uiManager,
                    _uiManager.GetUIController<PopupUIController>(),
                    _examine,
                    _transform,
                    this));

            // WD EDIT START
            _shouldLogInChat = _configManager.GetCVar(GoobCVars.LogInChat);
            _configManager.OnValueChanged(GoobCVars.LogInChat, log => { _shouldLogInChat = log; });
            // WD EDIT END
        }

        public override void Shutdown()
        {
            base.Shutdown();
            _overlay
                .RemoveOverlay<PopupOverlay>();
        }

        private void WrapAndRepeatPopup(PopupLabel existingLabel, string popupMessage)
        {
            existingLabel.TotalTime = 0;
            existingLabel.Repeats += 1;
            existingLabel.Text = Loc.GetString("popup-system-repeated-popup-stacking-wrap",
                ("popup-message", popupMessage),
                ("count", existingLabel.Repeats));
        }

        private void PopupMessage(string? message, PopupType type, EntityCoordinates coordinates, EntityUid? entity, bool recordReplay)
        {
            if (message == null)
                return;

            if (recordReplay && _replayRecording.IsRecording)
            {
                if (entity != null)
                    _replayRecording.RecordClientMessage(new PopupEntityEvent(message, type, GetNetEntity(entity.Value)));
                else
                    _replayRecording.RecordClientMessage(new PopupCoordinatesEvent(message, type, GetNetCoordinates(coordinates)));
            }

            // WD EDIT START
            var popupData = new WorldPopupData(message, type, coordinates, entity);
            if (_aliveWorldLabels.TryGetValue(popupData, out var existingLabel))
            {
                WrapAndRepeatPopup(existingLabel, popupData.Message);
                return;
            }

            var label = new WorldPopupLabel(coordinates)
            {
                Text = message,
                Type = type,
            };

            _aliveWorldLabels.Add(popupData, label);

            if (_shouldLogInChat &&
                _playerManager.LocalEntity != null &&
                _examine.InRangeUnOccluded(_playerManager.LocalEntity.Value, coordinates, 10))
            {
                var fontsize = FontSizeDict.GetValueOrDefault(type, "10");
                var fontcolor = type is PopupType.LargeCaution or PopupType.MediumCaution or PopupType.SmallCaution
                    ? "#C62828"
                    : "#AEABC4";

                var wrappedMessage = $"[font size={fontsize}][color={fontcolor}]{message}[/color][/font]";
                var chatMsg = new ChatMessage(ChatChannel.Emotes, message, wrappedMessage, GetNetEntity(EntityUid.Invalid), null);
                _uiManager.GetUIController<ChatUIController>().ProcessChatMessage(chatMsg);
            }
            // WD EDIT END
        }

        #region Abstract Method Implementations
        public override void PopupCoordinates(string? message, EntityCoordinates coordinates, PopupType type = PopupType.Small)
        {
            PopupMessage(message, type, coordinates, null, true);
        }

        public override void PopupCoordinates(string? message, EntityCoordinates coordinates, ICommonSession recipient, PopupType type = PopupType.Small)
        {
            if (_playerManager.LocalSession == recipient)
                PopupMessage(message, type, coordinates, null, true);
        }

        public override void PopupCoordinates(string? message, EntityCoordinates coordinates, EntityUid recipient, PopupType type = PopupType.Small)
        {
            if (_playerManager.LocalEntity == recipient)
                PopupMessage(message, type, coordinates, null, true);
        }

        public override void PopupPredictedCoordinates(string? message, EntityCoordinates coordinates, EntityUid? recipient, PopupType type = PopupType.Small)
        {
            if (recipient != null && _timing.IsFirstTimePredicted)
                PopupCoordinates(message, coordinates, recipient.Value, type);
        }

        private void PopupCursorInternal(string? message, PopupType type, bool recordReplay)
        {
            if (message == null)
                return;

            if (recordReplay && _replayRecording.IsRecording)
                _replayRecording.RecordClientMessage(new PopupCursorEvent(message, type));

            var popupData = new CursorPopupData(message, type);
            if (_aliveCursorLabels.TryGetValue(popupData, out var existingLabel))
            {
                WrapAndRepeatPopup(existingLabel, popupData.Message);
                return;
            }

            var label = new CursorPopupLabel(_inputManager.MouseScreenPosition)
            {
                Text = message,
                Type = type,
            };

            _aliveCursorLabels.Add(popupData, label);
        }

        public override void PopupCursor(string? message, PopupType type = PopupType.Small)
        {
            if (!_timing.IsFirstTimePredicted)
                return;

            PopupCursorInternal(message, type, true);
        }

        public override void PopupCursor(string? message, ICommonSession recipient, PopupType type = PopupType.Small)
        {
            if (_playerManager.LocalSession == recipient)
                PopupCursor(message, type);
        }

        public override void PopupCursor(string? message, EntityUid recipient, PopupType type = PopupType.Small)
        {
            if (_playerManager.LocalEntity == recipient)
                PopupCursor(message, type);
        }

        public override void PopupCoordinates(string? message, EntityCoordinates coordinates, Filter filter, bool replayRecord, PopupType type = PopupType.Small)
        {
            PopupCoordinates(message, coordinates, type);
        }

        public override void PopupEntity(string? message, EntityUid uid, EntityUid recipient, PopupType type = PopupType.Small)
        {
            if (_playerManager.LocalEntity == recipient)
                PopupEntity(message, uid, type);
        }

        public override void PopupEntity(string? message, EntityUid uid, ICommonSession recipient, PopupType type = PopupType.Small)
        {
            if (_playerManager.LocalSession == recipient)
                PopupEntity(message, uid, type);
        }

        public override void PopupEntity(string? message, EntityUid uid, Filter filter, bool recordReplay, PopupType type = PopupType.Small)
        {
            if (!filter.Recipients.Contains(_playerManager.LocalSession))
                return;

            PopupEntity(message, uid, type);
        }

        public override void PopupClient(string? message, EntityUid? recipient, PopupType type = PopupType.Small)
        {
            if (recipient == null)
                return;

            if (_timing.IsFirstTimePredicted)
                PopupCursor(message, recipient.Value, type);
        }

        public override void PopupClient(string? message, EntityUid uid, EntityUid? recipient, PopupType type = PopupType.Small)
        {
            if (recipient == null)
                return;

            if (_timing.IsFirstTimePredicted)
                PopupEntity(message, uid, recipient.Value, type);
        }

        public override void PopupClient(string? message, EntityCoordinates coordinates, EntityUid? recipient, PopupType type = PopupType.Small)
        {
            if (recipient == null)
                return;

            if (_timing.IsFirstTimePredicted)
                PopupCoordinates(message, coordinates, recipient.Value, type);
        }

        public override void PopupEntity(string? message, EntityUid uid, PopupType type = PopupType.Small)
        {
            if (TryComp(uid, out TransformComponent? transform))
                PopupMessage(message, type, transform.Coordinates, uid, true);
        }

        public override void PopupPredicted(string? message, EntityUid uid, EntityUid? recipient, PopupType type = PopupType.Small)
        {
            if (recipient != null && _timing.IsFirstTimePredicted)
                PopupEntity(message, uid, recipient.Value, type);
        }

        public override void PopupPredicted(string? recipientMessage, string? othersMessage, EntityUid uid, EntityUid? recipient, PopupType type = PopupType.Small)
        {
            if (recipient != null && _timing.IsFirstTimePredicted)
                PopupEntity(recipientMessage, uid, recipient.Value, type);
        }

        #endregion

        #region Network Event Handlers

        private void OnPopupCursorEvent(PopupCursorEvent ev)
        {
            PopupCursorInternal(ev.Message, ev.Type, false);
        }

        private void OnPopupCoordinatesEvent(PopupCoordinatesEvent ev)
        {
            PopupMessage(ev.Message, ev.Type, GetCoordinates(ev.Coordinates), null, false);
        }

        private void OnPopupEntityEvent(PopupEntityEvent ev)
        {
            var entity = GetEntity(ev.Uid);

            if (TryComp(entity, out TransformComponent? transform))
                PopupMessage(ev.Message, ev.Type, transform.Coordinates, entity, false);
        }

        private void OnRoundRestart(RoundRestartCleanupEvent ev)
        {
            _aliveCursorLabels.Clear();
            _aliveWorldLabels.Clear();
        }

        #endregion

        public static float GetPopupLifetime(PopupLabel label)
        {
            return Math.Clamp(PopupLifetimePerCharacter * label.Text.Length,
                MinimumPopupLifetime,
                MaximumPopupLifetime);
        }

        public override void FrameUpdate(float frameTime)
        {
            if (_aliveWorldLabels.Count == 0 && _aliveCursorLabels.Count == 0)
                return;

            if (_aliveWorldLabels.Count > 0)
            {
                var aliveWorldToRemove = new ValueList<WorldPopupData>();
                foreach (var (data, label) in _aliveWorldLabels)
                {
                    label.TotalTime += frameTime;
                    if (label.TotalTime > GetPopupLifetime(label) || Deleted(label.InitialPos.EntityId))
                    {
                        aliveWorldToRemove.Add(data);
                    }
                }
                foreach (var data in aliveWorldToRemove)
                {
                    _aliveWorldLabels.Remove(data);
                }
            }

            if (_aliveCursorLabels.Count > 0)
            {
                var aliveCursorToRemove = new ValueList<CursorPopupData>();
                foreach (var (data, label) in _aliveCursorLabels)
                {
                    label.TotalTime += frameTime;
                    if (label.TotalTime > GetPopupLifetime(label))
                    {
                        aliveCursorToRemove.Add(data);
                    }
                }
                foreach (var data in aliveCursorToRemove)
                {
                    _aliveCursorLabels.Remove(data);
                }
            }
        }

        public abstract class PopupLabel
        {
            public PopupType Type = PopupType.Small;
            public string Text { get; set; } = string.Empty;
            public float TotalTime { get; set; }
            public int Repeats = 1;
        }

        public sealed class WorldPopupLabel(EntityCoordinates coordinates) : PopupLabel
        {
            /// <summary>
            /// The original EntityCoordinates of the label.
            /// </summary>
            public EntityCoordinates InitialPos = coordinates;
        }

        public sealed class CursorPopupLabel(ScreenCoordinates screenCoords) : PopupLabel
        {
            public ScreenCoordinates InitialPos = screenCoords;
        }

        [UsedImplicitly]
        private record struct WorldPopupData(
            string Message,
            PopupType Type,
            EntityCoordinates Coordinates,
            EntityUid? Entity);

        [UsedImplicitly]
        private record struct CursorPopupData(
            string Message,
            PopupType Type);
    }
}
