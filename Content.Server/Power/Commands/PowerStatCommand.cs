// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Server.Power.EntitySystems;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Power.Commands
{
    [AdminCommand(AdminFlags.Debug)]
    public sealed class PowerStatCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _e = default!;

        public string Command => "powerstat";
        public string Description => "Shows statistics for pow3r";
        public string Help => "Usage: powerstat";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var stats = _e.System<PowerNetSystem>().GetStatistics();

            shell.WriteLine($"networks: {stats.CountNetworks}");
            shell.WriteLine($"loads: {stats.CountLoads}");
            shell.WriteLine($"supplies: {stats.CountSupplies}");
            shell.WriteLine($"batteries: {stats.CountBatteries}");
        }
    }
}