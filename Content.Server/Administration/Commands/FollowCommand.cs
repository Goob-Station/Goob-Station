// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LankLTE <135308300+LankLTE@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 crazybrain23 <44417085+crazybrain23@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Administration;
using Content.Shared.Follower;
using Robust.Shared.Console;
using Robust.Shared.Enums;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class FollowCommand : LocalizedEntityCommands
{
    [Dependency] private readonly FollowerSystem _followerSystem = default!;

    public override string Command => "follow";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player)
        {
            shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-need-exactly-one-argument"));
            return;
        }

        if (player.Status != SessionStatus.InGame || player.AttachedEntity is not { Valid: true } playerEntity)
        {
            shell.WriteError(Loc.GetString("shell-must-be-attached-to-entity"));
            return;
        }

        if (NetEntity.TryParse(args[0], out var uidNet) && EntityManager.TryGetEntity(uidNet, out var uid))
            _followerSystem.StartFollowingEntity(playerEntity, uid.Value);
    }
}