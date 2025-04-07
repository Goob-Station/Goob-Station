// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Alex Pavlenko <diraven@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Alice "Arimah" Heurlin <30327355+arimah@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Boaz1111 <149967078+Boaz1111@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Flareguy <78941145+Flareguy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ghagliiarghii <68826635+Ghagliiarghii@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 HS <81934438+HolySSSS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Redfire1331 <125223432+Redfire1331@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Rouge2t7 <81053047+Sarahon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 Truoizys <153248924+Truoizys@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TsjipTsjip <19798667+TsjipTsjip@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ubaser <134914314+UbaserB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 neutrino <67447925+neutrino-laser@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 redfire1331 <Redfire1331@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Арт <123451459+JustArt1m@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Runtime.InteropServices;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Players.RateLimiting;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Players.RateLimiting;

public sealed class PlayerRateLimitManager : SharedPlayerRateLimitManager
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private readonly Dictionary<string, RegistrationData> _registrations = new();
    private readonly Dictionary<ICommonSession, Dictionary<string, RateLimitDatum>> _rateLimitData = new();

    public override RateLimitStatus CountAction(ICommonSession player, string key)
    {
        if (player.Status == SessionStatus.Disconnected)
            throw new ArgumentException("Player is not connected");
        if (!_registrations.TryGetValue(key, out var registration))
            throw new ArgumentException($"Unregistered key: {key}");

        var playerData = _rateLimitData.GetOrNew(player);
        ref var datum = ref CollectionsMarshal.GetValueRefOrAddDefault(playerData, key, out _);
        var time = _gameTiming.RealTime;
        if (datum.CountExpires < time)
        {
            // Period expired, reset it.
            datum.CountExpires = time + registration.LimitPeriod;
            datum.Count = 0;
            datum.Announced = false;
        }

        datum.Count += 1;

        if (datum.Count <= registration.LimitCount)
            return RateLimitStatus.Allowed;

        // Breached rate limits, inform admins if configured.
        // Negative delays can be used to disable admin announcements.
        if (registration.AdminAnnounceDelay is {TotalSeconds: >= 0} cvarAnnounceDelay)
        {
            if (datum.NextAdminAnnounce < time)
            {
                registration.Registration.AdminAnnounceAction!(player);
                datum.NextAdminAnnounce = time + cvarAnnounceDelay;
            }
        }

        if (!datum.Announced)
        {
            registration.Registration.PlayerLimitedAction?.Invoke(player);
            _adminLog.Add(
                registration.Registration.AdminLogType,
                LogImpact.Medium,
                $"Player {player} breached '{key}' rate limit ");

            datum.Announced = true;
        }

        return RateLimitStatus.Blocked;
    }

    public override void Register(string key, RateLimitRegistration registration)
    {
        if (_registrations.ContainsKey(key))
            throw new InvalidOperationException($"Key already registered: {key}");

        var data = new RegistrationData
        {
            Registration = registration,
        };

        if ((registration.AdminAnnounceAction == null) != (registration.CVarAdminAnnounceDelay == null))
        {
            throw new ArgumentException(
                $"Must set either both {nameof(registration.AdminAnnounceAction)} and {nameof(registration.CVarAdminAnnounceDelay)} or neither");
        }

        _cfg.OnValueChanged(
            registration.CVarLimitCount,
            i => data.LimitCount = i,
            invokeImmediately: true);
        _cfg.OnValueChanged(
            registration.CVarLimitPeriodLength,
            i => data.LimitPeriod = TimeSpan.FromSeconds(i),
            invokeImmediately: true);

        if (registration.CVarAdminAnnounceDelay != null)
        {
            _cfg.OnValueChanged(
                registration.CVarAdminAnnounceDelay,
                i => data.AdminAnnounceDelay = TimeSpan.FromSeconds(i),
                invokeImmediately: true);
        }

        _registrations.Add(key, data);
    }

    public override void Initialize()
    {
        _playerManager.PlayerStatusChanged += PlayerManagerOnPlayerStatusChanged;
    }

    private void PlayerManagerOnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus == SessionStatus.Disconnected)
            _rateLimitData.Remove(e.Session);
    }

    private sealed class RegistrationData
    {
        public required RateLimitRegistration Registration { get; init; }
        public TimeSpan LimitPeriod { get; set; }
        public int LimitCount { get; set; }
        public TimeSpan? AdminAnnounceDelay { get; set; }
    }

    private struct RateLimitDatum
    {
        /// <summary>
        /// Time stamp (relative to <see cref="IGameTiming.RealTime"/>) this rate limit period will expire at.
        /// </summary>
        public TimeSpan CountExpires;

        /// <summary>
        /// How many actions have been done in the current rate limit period.
        /// </summary>
        public int Count;

        /// <summary>
        /// Have we announced to the player that they've been blocked in this rate limit period?
        /// </summary>
        public bool Announced;

        /// <summary>
        /// Time stamp (relative to <see cref="IGameTiming.RealTime"/>) of the
        /// next time we can send an announcement to admins about rate limit breach.
        /// </summary>
        public TimeSpan NextAdminAnnounce;
    }
}