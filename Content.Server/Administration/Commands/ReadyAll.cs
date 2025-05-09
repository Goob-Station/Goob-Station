// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Round)]
    public sealed class ReadyAll : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _e = default!;

        public string Command => "readyall";
        public string Description => "Readies up all players in the lobby, except for observers.";
        public string Help => $"{Command} | ̣{Command} <ready>";
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var ready = true;

            if (args.Length > 0)
            {
                ready = bool.Parse(args[0]);
            }

            var gameTicker = _e.System<GameTicker>();


            if (gameTicker.RunLevel != GameRunLevel.PreRoundLobby)
            {
                shell.WriteLine("This command can only be ran while in the lobby!");
                return;
            }

            gameTicker.ToggleReadyAll(ready);
        }
    }
}