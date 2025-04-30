// SPDX-FileCopyrightText: 2021 Daniel Castro Razo <eldanielcr@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.ContextMenu.UI;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Client.Commands;

public sealed class GroupingEntityMenuCommand : LocalizedCommands
{
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;

    public override string Command => "entitymenug";

    public override string Help => LocalizationManager.GetString($"cmd-{Command}-help", ("command", Command), ("groupingTypesCount", EntityMenuUIController.GroupingTypesCount));

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine(Help);
            return;
        }

        if (!int.TryParse(args[0], out var id))
        {
            shell.WriteError(LocalizationManager.GetString($"cmd-{Command}-error", ("arg", args[0])));
            return;
        }

        if (id < 0 || id > EntityMenuUIController.GroupingTypesCount - 1)
        {
            shell.WriteError(LocalizationManager.GetString($"cmd-{Command}-error", ("arg", args[0])));
            return;
        }

        var cvar = CCVars.EntityMenuGroupingType;

        _configurationManager.SetCVar(cvar, id);
        shell.WriteLine(LocalizationManager.GetString($"cmd-{Command}-notify", ("cvar", _configurationManager.GetCVar(cvar))));
    }
}