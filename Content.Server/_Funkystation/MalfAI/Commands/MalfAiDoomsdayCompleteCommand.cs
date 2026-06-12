// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared._Funkystation.MalfAI;
using Robust.Shared.Console;

namespace Content.Server._Funkystation.MalfAI.Commands;

[AdminCommand(AdminFlags.Fun)]
public sealed class MalfAiDoomsdayCompleteCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entMan = default!;

    public override string Command => "malfai_doomsday_complete";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var doomSys = _entMan.System<MalfAiDoomsdayRippleSystem>();

        var query = _entMan.EntityQueryEnumerator<MalfAiDoomsdayComponent, MalfAiMarkerComponent>();
        var found = false;
        while (query.MoveNext(out var ent, out var doom, out _))
        {
            doom.RemainingTime = TimeSpan.Zero;
            found = true;
        }

        if (!found)
            shell.WriteError("No active Malf AI doomsday found.");
        else
            shell.WriteLine("Doomsday countdown set to zero.");
    }
}
