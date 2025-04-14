// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Roles;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.Roles
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class ListRolesCommand : IConsoleCommand
    {
        public string Command => "listroles";

        public string Description => "Lists roles";

        public string Help => "listroles";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 0)
            {
                shell.WriteLine("Expected no arguments.");
                return;
            }

            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            foreach(var job in prototypeManager.EnumeratePrototypes<JobPrototype>())
            {
                shell.WriteLine(job.ID);
            }
        }
    }
}