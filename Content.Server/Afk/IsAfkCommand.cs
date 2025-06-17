// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.Afk
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class IsAfkCommand : LocalizedCommands
    {
        [Dependency] private readonly IAfkManager _afkManager = default!;
        [Dependency] private readonly IPlayerManager _players = default!;

        public override string Command => "isafk";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length == 0)
            {
                shell.WriteError(Loc.GetString($"shell-need-exactly-one-argument"));
                return;
            }

            if (!_players.TryGetSessionByUsername(args[0], out var player))
            {
                shell.WriteError(Loc.GetString($"shell-target-player-does-not-exist"));
                return;
            }

            shell.WriteLine(Loc.GetString(_afkManager.IsAfk(player) ? "cmd-isafk-true" : "cmd-isafk-false"));
        }

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                return CompletionResult.FromHintOptions(
                    CompletionHelper.SessionNames(players: _players),
                    "<playerName>");
            }

            return CompletionResult.Empty;
        }
    }
}