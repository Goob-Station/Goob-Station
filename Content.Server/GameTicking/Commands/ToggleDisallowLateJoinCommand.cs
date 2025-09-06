// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.GameTicking.Commands
{
    [AdminCommand(AdminFlags.Round)]
    public sealed class ToggleDisallowLateJoinCommand : LocalizedCommands
    {
        [Dependency] private readonly IConfigurationManager _configManager = default!;

        public override string Command => "toggledisallowlatejoin";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 1)
            {
                shell.WriteLine(Loc.GetString($"shell-need-exactly-one-argument"));
                return;
            }

            if (bool.TryParse(args[0], out var result))
            {
                _configManager.SetCVar(CCVars.GameDisallowLateJoins, bool.Parse(args[0]));
                shell.WriteLine(Loc.GetString(result ? "cmd-toggledisallowlatejoin-disabled" : "cmd-toggledisallowlatejoin-enabled"));
            }
            else
                shell.WriteLine(Loc.GetString($"shell-invalid-bool"));
        }
    }
}