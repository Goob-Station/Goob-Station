// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.Chat.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class SetOOCCommand : LocalizedCommands
{
    [Dependency] private readonly IConfigurationManager _configManager = default!;

    public override string Command => "setooc";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length > 1)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments", ("lower", 0), ("upper", 1)));
            return;
        }

        var ooc = _configManager.GetCVar(CCVars.OocEnabled);

        if (args.Length == 0)
        {
            ooc = !ooc;
        }

        if (args.Length == 1 && !bool.TryParse(args[0], out ooc))
        {
            shell.WriteError(Loc.GetString("shell-invalid-bool"));
            return;
        }

        _configManager.SetCVar(CCVars.OocEnabled, ooc);

        shell.WriteLine(Loc.GetString(ooc ? "cmd-setooc-ooc-enabled" : "cmd-setooc-ooc-disabled"));
    }
}