using Content.Server.Administration;
using Content.Shared._CorvaxGoob.Animation;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server._CorvaxGoob.Animation;

[AdminCommand(AdminFlags.Fun)]
public sealed class PlayAnimationCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public string Command => "playanimation";

    public string Description => "Plays animation on given entity.";

    public string Help => "playanimation <entityUid> <animation>";

    public void Execute(IConsoleShell shell, string arg, string[] args)
    {
        if (args.Length == 0 || args.Length < 2)
            return;

        if (!NetEntity.TryParse(args[0], out var targetUidNet) || !_entityManager.TryGetEntity(targetUidNet, out var targetEntity))
        {
            shell.WriteLine(Loc.GetString("shell-entity-uid-must-be-number"));
            return;
        }

        _entityManager.System<AnimationPlayerSystem>().PlayAnimation(targetEntity.Value, args[1]);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        switch (args.Length)
        {
            case 1:
                return CompletionResult.FromHint("<entityUid>");
            case 2:
                var prototypes = _prototypeManager.EnumeratePrototypes<AnimationPrototype>().OrderBy(p => p.ID);

                var options = new List<string>();
                foreach (var prototype in prototypes) options.Add(prototype.ID);

                return CompletionResult.FromHintOptions(options, "<animation>");
        }
        return CompletionResult.Empty;
    }
}
