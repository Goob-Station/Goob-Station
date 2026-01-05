// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class DumpReagentGuideText : LocalizedEntityCommands
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override string Command => "dumpreagentguidetext";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString($"shell-need-exactly-one-argument"));
            return;
        }

        if (!_prototype.TryIndex<ReagentPrototype>(args[0], out var reagent))
        {
            shell.WriteError(Loc.GetString($"shell-argument-must-be-prototype",
                ("index", args[0]),
                ("prototype", nameof(ReagentPrototype))));
            return;
        }

        if (reagent.Metabolisms is null)
        {
            shell.WriteLine(Loc.GetString($"cmd-dumpreagentguidetext-nothing-to-dump"));
            return;
        }

        foreach (var entry in reagent.Metabolisms.Values)
        {
            foreach (var effect in entry.Effects)
            {
                shell.WriteLine(effect.GuidebookEffectDescription(_prototype, EntityManager.EntitySysManager) ??
                                Loc.GetString($"cmd-dumpreagentguidetext-skipped", ("effect", effect.GetType())));
            }
        }
    }
}