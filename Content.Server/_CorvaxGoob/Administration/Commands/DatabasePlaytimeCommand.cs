using Content.Server.Administration;
using Content.Server.Administration.Commands;
using Content.Server.Database;
using Content.Shared.Administration;
using Content.Shared.Players.PlayTimeTracking;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server._CorvaxGoob.Administration.Commands;

[AdminCommand(AdminFlags.Permissions)]
public sealed class DatabasePlaytimeGetTrackerCommand : IConsoleCommand
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public string Command => "databaseplaytime_gettracker";
    public string Description => Loc.GetString("cmd-database-playtime_gettracker-desc");
    public string Help => Loc.GetString("cmd-database-playtime_gettracker-help", ("command", Command));

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError(Loc.GetString("cmd-database-playtime_gettracker-error-args"));
            return;
        }

        var playerRecord = await _db.GetPlayerRecordByUserName(args[0]);

        if (playerRecord is null)
        {
            shell.WriteError(Loc.GetString("cmd-database-playtime_gettracker-error-player-not-found"));
            return;
        }

        var playTimes = await _db.GetPlayTimes(playerRecord.UserId);

        for (var i = 1; i < args.Length; i++)
        {
            if (!_prototype.TryIndex<PlayTimeTrackerPrototype>(args[i], out var trackerProto))
            {
                shell.WriteError(Loc.GetString("cmd-database-playtime_gettracker-error-tracker-prototype"));
                continue;
            }

            var tracker = playTimes.Where(p => p.Tracker == trackerProto.ID).FirstOrDefault();

            var time = TimeSpan.Zero;

            if (tracker is not null)
                time = tracker.TimeSpent;

            shell.WriteLine(Loc.GetString("cmd-database-playtime_gettracker-succeed", ("username", playerRecord.LastSeenUserName), ("tracker", trackerProto.ID), ("time", time)));
        }
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHint(Loc.GetString("cmd-database-playtime_gettracker-arg-user"));
        }

        if (args.Length > 1)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.PrototypeIDs<PlayTimeTrackerPrototype>(),
                Loc.GetString("cmd-database-playtime_gettracker-arg-tracker"));
        }

        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Permissions)]
public sealed class DatabasePlaytimeAddTrackerTimeCommand : IConsoleCommand
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    public string Command => "databaseplaytime_addtracker";
    public string Description => Loc.GetString("cmd-database-playtime_addrole-desc");
    public string Help => Loc.GetString("cmd-database-playtime_addrole-help", ("command", Command));

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2 || (args.Length - 1) % 2 == 1)
        {
            shell.WriteError(Loc.GetString("cmd-database-playtime_addrole-error-args"));
            return;
        }

        var playerRecord = await _db.GetPlayerRecordByUserName(args[0]);

        if (playerRecord is null)
        {
            shell.WriteError(Loc.GetString("cmd-database-playtime_addrole-error-player-not-found"));
            return;
        }

        var updateList = new List<PlayTimeUpdate>();
        var updateDictionary = new Dictionary<string, TimeSpan>();

        var currentPlayTimes = await _db.GetPlayTimes(playerRecord.UserId);

        for (var i = 1; i < args.Length; i = i + 2)
        {
            if (!_prototype.TryIndex<PlayTimeTrackerPrototype>(args[i], out var trackerProto))
            {
                shell.WriteError(Loc.GetString("cmd-database-playtime_addrole-error-tracker-prototype"));
                continue;
            }

            var minutes = PlayTimeCommandUtilities.CountMinutes(args[i + 1]);

            var tracker = currentPlayTimes.Where(p => p.Tracker == trackerProto.ID).FirstOrDefault();

            var time = TimeSpan.Zero;
            var timeToAdd = TimeSpan.FromMinutes(minutes);

            if (tracker is not null)
                time = tracker.TimeSpent + timeToAdd;
            else
                time = timeToAdd;

            if (time.TotalMinutes < 0)
                time = TimeSpan.Zero;

            if (updateDictionary.ContainsKey(trackerProto.ID))
                updateDictionary[trackerProto.ID].Add(time);
            else
                updateDictionary.Add(trackerProto.ID, time);
        }

        foreach (var tracker in updateDictionary)
        {
            updateList.Add(new PlayTimeUpdate(playerRecord.UserId, tracker.Key, tracker.Value));

            shell.WriteLine(Loc.GetString("cmd-database-playtime_addrole-succeed", ("username", playerRecord.LastSeenUserName), ("tracker", tracker.Key), ("time", tracker.Value)));
        }

        await _db.UpdatePlayTimes(updateList);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHint(Loc.GetString("cmd-database-playtime_addrole-arg-user"));
        }

        if (args.Length > 1 && args.Length % 2 == 1)
        {
            return CompletionResult.FromHint(Loc.GetString("cmd-database-playtime_addrole-arg-time"));
        }

        if (args.Length > 1 && args.Length % 2 == 0)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.PrototypeIDs<PlayTimeTrackerPrototype>(),
                Loc.GetString("cmd-database-playtime_addrole-arg-tracker"));
        }

        return CompletionResult.Empty;
    }
}
