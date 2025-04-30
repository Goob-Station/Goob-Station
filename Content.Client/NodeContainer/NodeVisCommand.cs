// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Administration.Managers;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Client.NodeContainer
{
    public sealed class NodeVisCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _e = default!;

        public string Command => "nodevis";
        public string Description => "Toggles node group visualization";
        public string Help => "";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var adminMan = IoCManager.Resolve<IClientAdminManager>();
            if (!adminMan.HasFlag(AdminFlags.Debug))
            {
                shell.WriteError("You need +DEBUG for this command");
                return;
            }

            var sys = _e.System<NodeGroupSystem>();
            sys.SetVisEnabled(!sys.VisEnabled);
        }
    }

    public sealed class NodeVisFilterCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _e = default!;

        public string Command => "nodevisfilter";
        public string Description => "Toggles showing a specific group on nodevis";
        public string Help => "Usage: nodevis [filter]\nOmit filter to list currently masked-off";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var sys = _e.System<NodeGroupSystem>();

            if (args.Length == 0)
            {
                foreach (var filtered in sys.Filtered)
                {
                    shell.WriteLine(filtered);
                }
            }
            else
            {
                var filter = args[0];
                if (!sys.Filtered.Add(filter))
                {
                    sys.Filtered.Remove(filter);
                }
            }
        }
    }
}