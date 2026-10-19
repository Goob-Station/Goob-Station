// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration;
using Content.Server.MalfAI;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.MalfAI;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Console;

namespace Content.Server.MalfAI.Commands;

[AdminCommand(AdminFlags.Round)]
public sealed class MalfAiDoomsdayCompleteCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entMan = default!;

    public string Command => "malfai.doomsday_complete";
    public string Description => "Immediately completes the Malf AI Doomsday Protocol (fires completion).";
    public string Help => "Usage: malfai.doomsday_complete\nIf a countdown is active, it will complete for that AI; otherwise attempts to complete for any Station AI present.";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var stationSys = _entMan.System<StationSystem>();
        // Prefer any active doomsday components.
        var any = false;
        var query = _entMan.EntityQueryEnumerator<MalfAiDoomsdayComponent>();
        while (query.MoveNext(out var aiUid, out var dd))
        {
            // Even if not active, we can still attempt to use its stored station if set.
            var station = dd.Station != default ? dd.Station : stationSys.GetOwningStation(aiUid) ?? default;
            if (station == default)
                continue;

            var ripple = _entMan.System<MalfAiDoomsdayRippleSystem>();
            ripple.TriggerRippleAndEndRound(station, aiUid);
            any = true;
        }

        if (any)
        {
            shell.WriteLine("Triggered doomsday completion for existing Malf AI record(s).");
            return;
        }

        // Fallback: find any station AI and complete at its station.
        var aiQuery = _entMan.EntityQueryEnumerator<StationAiHeldComponent>();
        while (aiQuery.MoveNext(out var aiUid, out _))
        {
            var station = stationSys.GetOwningStation(aiUid);
            if (station == null)
                continue;
            var ripple2 = _entMan.System<MalfAiDoomsdayRippleSystem>();
            ripple2.TriggerRippleAndEndRound(station.Value, aiUid);
            any = true;
        }

        if (!any)
            shell.WriteError("No suitable AI found to complete doomsday.");
        else
            shell.WriteLine("Triggered doomsday completion for AI.");
    }
}
