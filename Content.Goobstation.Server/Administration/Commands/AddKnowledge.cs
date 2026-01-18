using System.Linq;
using Content.Goobstation.Common.Knowledge.Systems;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class AddKnowledge : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override string Command => "addknowledge";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var knowledgeSystem = _entMan.System<KnowledgeSystem>();

        if (args.Length < 2)
        {
            shell.WriteLine(Loc.GetString("cmd-addknowledge-args-error"));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var targetNet)
            || !_entMan.TryGetEntity(targetNet, out var targetEntity))
        {
            shell.WriteLine(Loc.GetString("cmd-addknowledge-bad-target", ("target", args[0])));
            return;
        }

        var target  = targetEntity.Value;
        knowledgeSystem.TryEnsureKnowledgeUnit(target, args[1], out var knowledge);

        if (knowledge != null)
        {
            shell.WriteLine(Loc.GetString("cmd-addknowledge-success",
                ("knowledge", _entMan.ToPrettyString(knowledge)),
                ("target", _entMan.ToPrettyString(target))));
        }
        else
        {
            shell.WriteLine(Loc.GetString("cmd-addknowledge-failure",
                ("knowledge", args[1]),
                ("target", _entMan.ToPrettyString(target))));
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length != 2)
            return CompletionResult.Empty;

        var options = _proto.EnumeratePrototypes<EntityPrototype>()
            .Where(p => p.Components.TryGetComponent("Knowledge", out _))
            .Select(c => c.ID)
            .OrderBy(c => c)
            .ToArray();

        return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-addknowledge-hint"));
    }
}
