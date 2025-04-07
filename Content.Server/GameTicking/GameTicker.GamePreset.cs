// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kot <1192090+koteq@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2023 qwerltaz <69696513+qwerltaz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 OctoRocket <88291550+OctoRocket@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <shadowjjt@gmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.GameTicking.Presets;
using Content.Server.Maps;
using Content.Shared.CCVar;
using JetBrains.Annotations;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Content.Server.GameTicking
{
    public sealed partial class GameTicker
    {
        public const float PresetFailedCooldownIncrease = 30f;

        /// <summary>
        /// The selected preset that will be used at the start of the next round.
        /// </summary>
        public GamePresetPrototype? Preset { get; private set; }

        /// <summary>
        /// The preset that's currently active.
        /// </summary>
        public GamePresetPrototype? CurrentPreset { get; private set; }

        private bool StartPreset(ICommonSession[] origReadyPlayers, bool force)
        {
            var startAttempt = new RoundStartAttemptEvent(origReadyPlayers, force);
            RaiseLocalEvent(startAttempt);

            if (!startAttempt.Cancelled)
                return true;

            var presetTitle = CurrentPreset != null ? Loc.GetString(CurrentPreset.ModeTitle) : string.Empty;

            void FailedPresetRestart()
            {
                SendServerMessage(Loc.GetString("game-ticker-start-round-cannot-start-game-mode-restart",
                    ("failedGameMode", presetTitle)));
                RestartRound();
                DelayStart(TimeSpan.FromSeconds(PresetFailedCooldownIncrease));
            }

            if (_cfg.GetCVar(CCVars.GameLobbyFallbackEnabled))
            {
                var fallbackPresets = _cfg.GetCVar(CCVars.GameLobbyFallbackPreset).Split(",");
                var startFailed = true;

                foreach (var preset in fallbackPresets)
                {
                    ClearGameRules();
                    SetGamePreset(preset);
                    AddGamePresetRules();
                    StartGamePresetRules();

                    startAttempt.Uncancel();
                    RaiseLocalEvent(startAttempt);

                    if (!startAttempt.Cancelled)
                    {
                        _chatManager.SendAdminAnnouncement(
                            Loc.GetString("game-ticker-start-round-cannot-start-game-mode-fallback",
                                ("failedGameMode", presetTitle),
                                ("fallbackMode", Loc.GetString(preset))));
                        RefreshLateJoinAllowed();
                        startFailed = false;
                        break;
                    }
                }

                if (startFailed)
                {
                    FailedPresetRestart();
                    return false;
                }
            }

            else
            {
                FailedPresetRestart();
                return false;
            }

            return true;
        }

        private void InitializeGamePreset()
        {
            SetGamePreset(LobbyEnabled ? _cfg.GetCVar(CCVars.GameLobbyDefaultPreset) : "sandbox");
        }

        public void SetGamePreset(GamePresetPrototype? preset, bool force = false)
        {
            // Do nothing if this game ticker is a dummy!
            if (DummyTicker)
                return;

            Preset = preset;
            ValidateMap();
            UpdateInfoText();

            if (force)
            {
                StartRound(true);
            }
        }

        public void SetGamePreset(string preset, bool force = false)
        {
            var proto = FindGamePreset(preset);
            if(proto != null)
                SetGamePreset(proto, force);
        }

        public GamePresetPrototype? FindGamePreset(string preset)
        {
            if (_prototypeManager.TryIndex(preset, out GamePresetPrototype? presetProto))
                return presetProto;

            foreach (var proto in _prototypeManager.EnumeratePrototypes<GamePresetPrototype>())
            {
                foreach (var alias in proto.Alias)
                {
                    if (preset.Equals(alias, StringComparison.InvariantCultureIgnoreCase))
                        return proto;
                }
            }

            return null;
        }

        public bool TryFindGamePreset(string preset, [NotNullWhen(true)] out GamePresetPrototype? prototype)
        {
            prototype = FindGamePreset(preset);

            return prototype != null;
        }

        public bool IsMapEligible(GameMapPrototype map)
        {
            if (Preset == null)
                return true;

            if (Preset.MapPool == null || !_prototypeManager.TryIndex<GameMapPoolPrototype>(Preset.MapPool, out var pool))
                return true;

            return pool.Maps.Contains(map.ID);
        }

        private void ValidateMap()
        {
            if (Preset == null || _gameMapManager.GetSelectedMap() is not { } map)
                return;

            if (Preset.MapPool == null ||
                !_prototypeManager.TryIndex<GameMapPoolPrototype>(Preset.MapPool, out var pool))
                return;

            if (pool.Maps.Contains(map.ID))
                return;

            _gameMapManager.SelectMapRandom();
        }

        [PublicAPI]
        private bool AddGamePresetRules()
        {
            if (DummyTicker || Preset == null)
                return false;

            CurrentPreset = Preset;
            foreach (var rule in Preset.Rules)
            {
                AddGameRule(rule);
            }

            return true;
        }

        public void StartGamePresetRules()
        {
            // May be touched by the preset during init.
            var rules = new List<EntityUid>(GetAddedGameRules());
            foreach (var rule in rules)
            {
                StartGameRule(rule);
            }
        }

        private void IncrementRoundNumber()
        {
            var playerIds = _playerGameStatuses.Keys.Select(player => player.UserId).ToArray();
            var serverName = _cfg.GetCVar(CCVars.AdminLogsServerName);

            // TODO FIXME AAAAAAAAAAAAAAAAAAAH THIS IS BROKEN
            // Task.Run as a terrible dirty workaround to avoid synchronization context deadlock from .Result here.
            // This whole setup logic should be made asynchronous so we can properly wait on the DB AAAAAAAAAAAAAH
            var task = Task.Run(async () =>
            {
                var server = await _dbEntryManager.ServerEntity;
                return await _db.AddNewRound(server, playerIds);
            });

            _taskManager.BlockWaitOnTask(task);
            RoundId = task.GetAwaiter().GetResult();
        }
    }
}