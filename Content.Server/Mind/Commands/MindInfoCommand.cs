// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.Mind.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class MindInfoCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly SharedRoleSystem _roles = default!;
        [Dependency] private readonly SharedMindSystem _minds = default!;

        public override string Command => "mindinfo";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 1)
            {
                shell.WriteLine(Loc.GetString($"shell-need-exactly-one-argument"));
                return;
            }

            if (!_playerManager.TryGetSessionByUsername(args[0], out var session))
            {
                shell.WriteLine(Loc.GetString($"cmd-mindinfo-mind-not-found"));
                return;
            }

            if (!_minds.TryGetMind(session, out var mindId, out var mind))
            {
                shell.WriteLine(Loc.GetString($"cmd-mindinfo-mind-not-found"));
                return;
            }

            var builder = new StringBuilder();
            builder.AppendFormat("player: {0}, mob: {1}\nroles: ", mind.UserId, mind.OwnedEntity);

            foreach (var role in _roles.MindGetAllRoleInfo(mindId))
            {
                builder.AppendFormat("{0} ", role.Name);
            }

            shell.WriteLine(builder.ToString());
        }
    }
}