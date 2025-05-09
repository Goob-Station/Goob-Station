// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 flyingkarii <123355664+flyingkarii@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Administration;
using Content.Shared.Players.PlayTimeTracking;
using Robust.Server.Player;
using Robust.Shared.Console;
using System.Text.RegularExpressions;

namespace Content.Server.Administration.Commands;

public sealed class PlayTimeCommandUtilities
{
    private readonly static Dictionary<string, int> Units = new() {
        { "y", 525960 },
        { "mo", 43800 },
        { "w", 10080 },
        { "d", 1440 },
        { "h", 60 },
        { "m", 1 },
    };

    public struct TimeUnit
    {
        public int TimeValue { get; }
        public string Unit { get; }

        public TimeUnit(int timeValue)
        {
            TimeValue = timeValue;
            Unit = "m";
        }

        public TimeUnit(int timeValue, string unit)
        {
            TimeValue = timeValue;
            Unit = unit;
        }
        public int ToMinutes()
        {
            var unitExists = Units.TryGetValue(Unit, out int minutes);

            if (!unitExists)
                return TimeValue;

            return TimeValue * minutes;
        }
    }

    public static List<TimeUnit> ConvertToTimeUnits(string timeString)
    {
        // Searching for something similar to 365d24h, etc.
        List<TimeUnit> result = new();

        // We want to support plain numbers as a translation to just minutes, just in case people don't know things like 30d or 1d are an option.
        if (int.TryParse(timeString, out int timeValue))
        {
            result.Add(new TimeUnit(timeValue, "m"));
            return result;
        }

        MatchCollection timeRegex = Regex.Matches(timeString, "(\\d+)([A-Za-z]+)");

        foreach (Match match in timeRegex)
        {
            bool isTimeAmountNumber = int.TryParse(match.Groups[1].Value, out int amountOfTime);
            string timeUnit = match.Groups[2].Value;

            if (!isTimeAmountNumber)
                continue;

            if (!Units.ContainsKey(timeUnit))
                continue;

            result.Add(new TimeUnit(amountOfTime, timeUnit));
        }

        return result;
    }

    public static int CountMinutes(string timeString)
    {
        List<TimeUnit> timeUnits = ConvertToTimeUnits(timeString);
        int total = 0;

        foreach (var timeUnit in timeUnits)
        {
            total += timeUnit.ToMinutes();
        }

        return total;
    }
}

[AdminCommand(AdminFlags.Moderator)]
public sealed class PlayTimeAddOverallCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;

    public string Command => "playtime_addoverall";
    public string Description => Loc.GetString("cmd-playtime_addoverall-desc");
    public string Help => Loc.GetString("cmd-playtime_addoverall-help", ("command", Command));

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("cmd-playtime_addoverall-error-args"));
            return;
        }

        var minutes = PlayTimeCommandUtilities.CountMinutes(args[1]);

        if (!_playerManager.TryGetSessionByUsername(args[0], out var player))
        {
            shell.WriteError(Loc.GetString("parse-session-fail", ("username", args[0])));
            return;
        }

        _playTimeTracking.AddTimeToOverallPlaytime(player, TimeSpan.FromMinutes(minutes));
        var overall = _playTimeTracking.GetOverallPlaytime(player);

        shell.WriteLine(Loc.GetString(
            "cmd-playtime_addoverall-succeed",
            ("username", args[0]),
            ("time", overall)));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(),
                Loc.GetString("cmd-playtime_addoverall-arg-user"));

        if (args.Length == 2)
            return CompletionResult.FromHint(Loc.GetString("cmd-playtime_addoverall-arg-minutes"));

        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Moderator)]
public sealed class PlayTimeAddRoleCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;

    public string Command => "playtime_addrole";
    public string Description => Loc.GetString("cmd-playtime_addrole-desc");
    public string Help => Loc.GetString("cmd-playtime_addrole-help", ("command", Command));

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 3)
        {
            shell.WriteError(Loc.GetString("cmd-playtime_addrole-error-args"));
            return;
        }

        var userName = args[0];
        if (!_playerManager.TryGetSessionByUsername(userName, out var player))
        {
            shell.WriteError(Loc.GetString("parse-session-fail", ("username", userName)));
            return;
        }

        var role = args[1];

        var m = PlayTimeCommandUtilities.CountMinutes(args[2]);

        _playTimeTracking.AddTimeToTracker(player, role, TimeSpan.FromMinutes(m));
        var time = _playTimeTracking.GetPlayTimeForTracker(player, role);
        shell.WriteLine(Loc.GetString("cmd-playtime_addrole-succeed",
            ("username", userName),
            ("role", role),
            ("time", time)));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.SessionNames(players: _playerManager),
                Loc.GetString("cmd-playtime_addrole-arg-user"));
        }

        if (args.Length == 2)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.PrototypeIDs<PlayTimeTrackerPrototype>(),
                Loc.GetString("cmd-playtime_addrole-arg-role"));
        }

        if (args.Length == 3)
            return CompletionResult.FromHint(Loc.GetString("cmd-playtime_addrole-arg-minutes"));

        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Moderator)]
public sealed class PlayTimeGetOverallCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;

    public string Command => "playtime_getoverall";
    public string Description => Loc.GetString("cmd-playtime_getoverall-desc");
    public string Help => Loc.GetString("cmd-playtime_getoverall-help", ("command", Command));

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("cmd-playtime_getoverall-error-args"));
            return;
        }

        var userName = args[0];
        if (!_playerManager.TryGetSessionByUsername(userName, out var player))
        {
            shell.WriteError(Loc.GetString("parse-session-fail", ("username", userName)));
            return;
        }

        var value = _playTimeTracking.GetOverallPlaytime(player);
        shell.WriteLine(Loc.GetString(
            "cmd-playtime_getoverall-success",
            ("username", userName),
            ("time", value)));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.SessionNames(players: _playerManager),
                Loc.GetString("cmd-playtime_getoverall-arg-user"));
        }

        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Moderator)]
public sealed class PlayTimeGetRoleCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;

    public string Command => "playtime_getrole";
    public string Description => Loc.GetString("cmd-playtime_getrole-desc");
    public string Help => Loc.GetString("cmd-playtime_getrole-help", ("command", Command));

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length is not (1 or 2))
        {
            shell.WriteLine(Loc.GetString("cmd-playtime_getrole-error-args"));
            return;
        }

        var userName = args[0];
        if (!_playerManager.TryGetSessionByUsername(userName, out var session))
        {
            shell.WriteError(Loc.GetString("parse-session-fail", ("username", userName)));
            return;
        }

        if (args.Length == 1)
        {
            var timers = _playTimeTracking.GetTrackerTimes(session);

            if (timers.Count == 0)
            {
                shell.WriteLine(Loc.GetString("cmd-playtime_getrole-no"));
                return;
            }

            foreach (var (role, time) in timers)
            {
                shell.WriteLine(Loc.GetString("cmd-playtime_getrole-role", ("role", role), ("time", time)));
            }
        }

        if (args.Length >= 2)
        {
            if (args[1] == "Overall")
            {
                var timer = _playTimeTracking.GetOverallPlaytime(session);
                shell.WriteLine(Loc.GetString("cmd-playtime_getrole-overall", ("time", timer)));
                return;
            }

            var time = _playTimeTracking.GetPlayTimeForTracker(session, args[1]);
            shell.WriteLine(Loc.GetString("cmd-playtime_getrole-succeed", ("username", session.Name),
                ("time", time)));
        }
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.SessionNames(players: _playerManager),
                Loc.GetString("cmd-playtime_getrole-arg-user"));
        }

        if (args.Length == 2)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.PrototypeIDs<PlayTimeTrackerPrototype>(),
                Loc.GetString("cmd-playtime_getrole-arg-role"));
        }

        return CompletionResult.Empty;
    }
}

/// <summary>
/// Saves the timers for a particular player immediately
/// </summary>
[AdminCommand(AdminFlags.Moderator)]
public sealed class PlayTimeSaveCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;

    public string Command => "playtime_save";
    public string Description => Loc.GetString("cmd-playtime_save-desc");
    public string Help => Loc.GetString("cmd-playtime_save-help", ("command", Command));

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine(Loc.GetString("cmd-playtime_save-error-args"));
            return;
        }

        var name = args[0];
        if (!_playerManager.TryGetSessionByUsername(name, out var pSession))
        {
            shell.WriteError(Loc.GetString("parse-session-fail", ("username", name)));
            return;
        }

        _playTimeTracking.SaveSession(pSession);
        shell.WriteLine(Loc.GetString("cmd-playtime_save-succeed", ("username", name)));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.SessionNames(players: _playerManager),
                Loc.GetString("cmd-playtime_save-arg-user"));
        }

        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Debug)]
public sealed class PlayTimeFlushCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;

    public string Command => "playtime_flush";
    public string Description => Loc.GetString("cmd-playtime_flush-desc");
    public string Help => Loc.GetString("cmd-playtime_flush-help", ("command", Command));

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length is not (0 or 1))
        {
            shell.WriteError(Loc.GetString("cmd-playtime_flush-error-args"));
            return;
        }

        if (args.Length == 0)
        {
            _playTimeTracking.FlushAllTrackers();
            return;
        }

        var name = args[0];
        if (!_playerManager.TryGetSessionByUsername(name, out var pSession))
        {
            shell.WriteError(Loc.GetString("parse-session-fail", ("username", name)));
            return;
        }

        _playTimeTracking.FlushTracker(pSession);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.SessionNames(players: _playerManager),
                Loc.GetString("cmd-playtime_flush-arg-user"));
        }

        return CompletionResult.Empty;
    }
}