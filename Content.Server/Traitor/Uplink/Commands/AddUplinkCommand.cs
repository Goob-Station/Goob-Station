// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 Alexander Evgrashin <evgrashin.adl@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <vincefvanwijk@gmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Server.Traitor.Uplink.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class AddUplinkCommand : LocalizedEntityCommands
{
    [Dependency] private readonly UplinkSystem _uplinkSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override string Command => "adduplink";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length > 3)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        ICommonSession? session;
        if (args.Length > 0)
        {
            // Get player entity
            if (!_playerManager.TryGetSessionByUsername(args[0], out session))
            {
                shell.WriteLine(Loc.GetString("shell-target-player-does-not-exist"));
                return;
            }
        }
        else
            session = shell.Player;

        if (session?.AttachedEntity is not { } user)
        {
            shell.WriteLine(Loc.GetString("add-uplink-command-error-1"));
            return;
        }

        // Get target item
        EntityUid? uplinkEntity = null;
        if (args.Length >= 2)
        {
            if (!int.TryParse(args[1], out var itemId))
            {
                shell.WriteLine(Loc.GetString("shell-entity-uid-must-be-number"));
                return;
            }

            var eNet = new NetEntity(itemId);

            if (!EntityManager.TryGetEntity(eNet, out var eUid))
            {
                shell.WriteLine(Loc.GetString("shell-invalid-entity-id"));
                return;
            }

            uplinkEntity = eUid;
        }

        var isDiscounted = false;
        if (args.Length >= 3)
        {
            if (!bool.TryParse(args[2], out isDiscounted))
            {
                shell.WriteLine(Loc.GetString("shell-invalid-bool"));
                return;
            }
        }

        // Finally add uplink
        if (!_uplinkSystem.AddUplink(user, 100, uplinkEntity: uplinkEntity)) // Goob edit - 100 TC
            shell.WriteLine(Loc.GetString("add-uplink-command-error-2"));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("add-uplink-command-completion-1")),
            2 => CompletionResult.FromHint(Loc.GetString("add-uplink-command-completion-2")),
            3 => CompletionResult.FromHint(Loc.GetString("add-uplink-command-completion-3")),
            _ => CompletionResult.Empty,
        };
    }
}
