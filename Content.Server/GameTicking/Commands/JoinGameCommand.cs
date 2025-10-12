// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Morber <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Repo <47093363+Titian3@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Managers;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Commands
{
    [AnyCommand]
    sealed class JoinGameCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IAdminManager _adminManager = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        public string Command => "joingame";
        public string Description => "";
        public string Help => "";

        public JoinGameCommand()
        {
            IoCManager.InjectDependencies(this);
        }
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var player = shell.Player;

            if (player == null)
            {
                return;
            }

            var ticker = _entManager.System<GameTicker>();
            var stationJobs = _entManager.System<StationJobsSystem>();

            if (ticker.PlayerGameStatuses.TryGetValue(player.UserId, out var status) && status == PlayerGameStatus.JoinedGame)
            {
                Logger.InfoS("security", $"{player.Name} ({player.UserId}) attempted to latejoin while in-game.");
                shell.WriteError($"{player.Name} is not in the lobby.   This incident will be reported.");
                return;
            }

            if (ticker.RunLevel == GameRunLevel.PreRoundLobby)
            {
                shell.WriteLine("Round has not started.");
                return;
            }
            else if (ticker.RunLevel == GameRunLevel.InRound)
            {
                string id = args[0];

                if (!int.TryParse(args[1], out var sid))
                {
                    shell.WriteError(Loc.GetString("shell-argument-must-be-number"));
                }

                var station = _entManager.GetEntity(new NetEntity(sid));
                var jobPrototype = _prototypeManager.Index<JobPrototype>(id);
                if(stationJobs.TryGetJobSlot(station, jobPrototype, out var slots) == false || slots == 0)
                {
                    shell.WriteLine($"{jobPrototype.LocalizedName} has no available slots.");
                    return;
                }

                if (_adminManager.IsAdmin(player) && _cfg.GetCVar(CCVars.AdminDeadminOnJoin))
                {
                    _adminManager.DeAdmin(player);
                }

                ticker.MakeJoinGame(player, station, id);
                return;
            }

            ticker.MakeJoinGame(player, EntityUid.Invalid);
        }
    }
}