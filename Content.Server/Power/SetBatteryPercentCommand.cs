// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Power
{
    [AdminCommand(AdminFlags.Debug)]
    public sealed class SetBatteryPercentCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entManager = default!;

        public string Command => "setbatterypercent";
        public string Description => "Drains or recharges a battery by entity uid and percentage, i.e.: forall with Battery do setbatterypercent $ID 0";
        public string Help => $"{Command} <id> <percent>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteLine($"Invalid amount of arguments.\n{Help}");
                return;
            }

            if (!NetEntity.TryParse(args[0], out var netEnt) || !_entManager.TryGetEntity(netEnt, out var id))
            {
                shell.WriteLine($"{args[0]} is not a valid entity id.");
                return;
            }

            if (!float.TryParse(args[1], out var percent))
            {
                shell.WriteLine($"{args[1]} is not a valid float (percentage).");
                return;
            }

            if (!_entManager.TryGetComponent<BatteryComponent>(id, out var battery))
            {
                shell.WriteLine($"No battery found with id {id}.");
                return;
            }
            var system = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<BatterySystem>();
            system.SetCharge(id.Value, battery.MaxCharge * percent / 100, battery);
            // Don't acknowledge b/c people WILL forall this
        }
    }
}