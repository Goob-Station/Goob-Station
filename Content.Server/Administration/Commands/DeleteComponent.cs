// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Spawn)]
    public sealed class DeleteComponent : LocalizedEntityCommands
    {
        [Dependency] private readonly IComponentFactory _compFactory = default!;

        public override string Command => "deletecomponent";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    shell.WriteLine(Loc.GetString($"shell-need-exactly-one-argument"));
                    break;
                default:
                    var name = string.Join(" ", args);

                    if (!_compFactory.TryGetRegistration(name, out var registration))
                    {
                        shell.WriteLine(Loc.GetString($"cmd-deletecomponent-no-component-exists", ("name", name)));
                        break;
                    }

                    var componentType = registration.Type;
                    var components = EntityManager.GetAllComponents(componentType, true);

                    var i = 0;

                    foreach (var (uid, component) in components)
                    {
                        EntityManager.RemoveComponent(uid, component);
                        i++;
                    }

                    shell.WriteLine(Loc.GetString($"cmd-deletecomponent-success", ("count", i), ("name", name)));

                    break;
            }
        }
    }
}