// SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration.Managers;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Stealth)]
public sealed class StealthminCommand : LocalizedCommands
{
    [Dependency] private readonly IAdminManager _adminManager = default!;

    public override string Command => "stealthmin";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player;
        if (player == null)
        {
            shell.WriteLine(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        var adminData = _adminManager.GetAdminData(player);

        DebugTools.AssertNotNull(adminData);

        if (!adminData!.Stealth)
            _adminManager.Stealth(player);
        else
            _adminManager.UnStealth(player);
    }
}