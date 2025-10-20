// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Administration;
using Content.Server.Implants;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class AddImplant : LocalizedCommands
{
    private const string CommandName = "addimplant";
    public override string Command => CommandName;

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var implantSystem = entityManager.System<SubdermalImplantSystem>();

        if (args.Length < 2)
        {
            shell.WriteLine(Loc.GetString("cmd-addimplant-args-error"));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var targetNet)
            || !entityManager.TryGetEntity(targetNet, out var targetEntity))
        {
            shell.WriteLine(Loc.GetString("cmd-addimplant-bad-target", ("target", args[0])));
            return;
        }

        var target  = targetEntity.Value;
        var implant = implantSystem.AddImplant(target, args[1]);

        if (implant != null)
        {
            shell.WriteLine(Loc.GetString("cmd-addimplant-success",
                ("implant", entityManager.ToPrettyString(implant)),
                ("target", entityManager.ToPrettyString(target))));
        }
        else
        {
            shell.WriteLine(Loc.GetString("cmd-addimplant-failure",
                ("implant", args[1]),
                ("target", entityManager.ToPrettyString(target))));
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();

        if (args.Length != 2)
            return CompletionResult.Empty;

        var options = prototypeManager.EnumeratePrototypes<EntityPrototype>()
            .Where(p => p.Components.TryGetComponent("SubdermalImplant", out _))
            .Select(c => c.ID)
            .OrderBy(c => c)
            .ToArray();

        return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-addimplant-hint"));
    }
}

