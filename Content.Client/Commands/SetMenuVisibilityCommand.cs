// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Client.Verbs;
using Content.Shared.Verbs;
using JetBrains.Annotations;
using Robust.Shared.Console;

namespace Content.Client.Commands;

[UsedImplicitly]
internal sealed class SetMenuVisibilityCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public override string Command => "menuvis";

    public override string Help => LocalizationManager.GetString($"cmd-{Command}-help", ("command", Command));

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (!TryParseArguments(shell, args, out var visibility))
            return;

        _entitySystemManager.GetEntitySystem<VerbSystem>().Visibility = visibility;
    }

    private bool TryParseArguments(IConsoleShell shell, string[] args, out MenuVisibility visibility)
    {
        visibility = MenuVisibility.Default;

        foreach (var arg in args)
        {
            switch (arg.ToLower())
            {
                // ReSharper disable once StringLiteralTypo
                case "nofov":
                    visibility |= MenuVisibility.NoFov;
                    break;
                // ReSharper disable once StringLiteralTypo
                case "incontainer":
                    visibility |= MenuVisibility.InContainer;
                    break;
                case "invisible":
                    visibility |= MenuVisibility.Invisible;
                    break;
                case "all":
                    visibility |= MenuVisibility.All;
                    break;
                default:
                    shell.WriteError(LocalizationManager.GetString($"cmd-{Command}-error", ("arg", arg)));
                    return false;
            }
        }

        return true;
    }
}