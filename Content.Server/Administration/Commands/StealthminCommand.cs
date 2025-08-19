// SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration.Managers;
using Content.Shared.Administration;
using JetBrains.Annotations;
using Robust.Shared.Console;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Commands;

[UsedImplicitly]
[AdminCommand(AdminFlags.Stealth)]
public sealed class StealthminCommand : LocalizedCommands
{
    public override string Command => "stealthmin";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
            var player = shell.Player;
            if (player == null)
            {
                shell.WriteLine(Loc.GetString("cmd-stealthmin-no-console"));
                return;
            }

            var mgr = IoCManager.Resolve<IAdminManager>();

            var adminData = mgr.GetAdminData(player);

            DebugTools.AssertNotNull(adminData);

            if (!adminData!.Stealth)
            {
                mgr.Stealth(player);
            }
            else
            {
                mgr.UnStealth(player);
            }
    }
}