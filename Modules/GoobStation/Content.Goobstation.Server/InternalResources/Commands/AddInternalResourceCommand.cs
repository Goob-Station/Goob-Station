using Content.Goobstation.Shared.InternalResources.Data;
using Content.Goobstation.Shared.InternalResources.EntitySystems;
using Content.Server.Administration;
using Content.Shared.Actions.Components;
using Content.Shared.Administration;
using Content.Shared.Prototypes;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using System;
using System.Linq;


namespace Content.Goobstation.Server.InternalResources.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class AddInternalResourceCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedInternalResourcesSystem _internalResources = default!;

    public override string Command => "addinternalresource";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("cmd-addinternalresource-invalid-args"));
        }

        if (!NetEntity.TryParse(args[0], out var targetUidNet) || !EntityManager.TryGetEntity(targetUidNet, out var targetEntity))
        {
            shell.WriteLine(Loc.GetString("shell-entity-uid-must-be-number"));
            return;
        }

        if (!_prototypeManager.TryIndex<InternalResourcesPrototype>(args[1], out var proto))
        {
            shell.WriteError(Loc.GetString("cmd-addinternalresource-type-not-found", ("type", args[1])));
            return;
        }

        _internalResources.EnsureInternalResources(targetEntity.Value, proto, out _);
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHint(Loc.GetString("cmd-addinternalresource-player-completion"));
        }

        if (args.Length != 2)
            return CompletionResult.Empty;

        var resourcesPrototypes = _prototypeManager.EnumeratePrototypes<InternalResourcesPrototype>().Select(p => p.ID); ;

        return CompletionResult.FromHintOptions(resourcesPrototypes, Loc.GetString("cmd-addinternalresource-type-completion"));
    }
}
