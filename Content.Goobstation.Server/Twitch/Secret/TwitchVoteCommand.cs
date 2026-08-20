using System.Linq;
using Content.Server.Administration;
using Content.Server.StationEvents.Components;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Twitch.Secret;

[AdminCommand(AdminFlags.Fun)]
public sealed class TwitchVoteCommand : LocalizedCommands
{
    public override string Command => "twitchvote";
    public override string Description => "Starts a Twitch Secret event vote.";
    public override string Help => "Usage: twitchvote [event1 event2 event3]";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 0 && args.Length != 3)
        {
            shell.WriteError(Help);
            return;
        }

        var entityManager = IoCManager.Resolve<IEntityManager>();
        var twitchSecret = entityManager.System<TwitchSecretSystem>();
        if (!twitchSecret.TryOpenAdminVote(args, out var error))
        {
            shell.WriteError(error);
            return;
        }

        shell.WriteLine("Twitch event vote opened.");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length is < 1 or > 3)
            return CompletionResult.Empty;

        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var componentFactory = IoCManager.Resolve<IComponentFactory>();
        var options = prototypeManager.EnumeratePrototypes<EntityPrototype>()
            .Where(prototype =>
                !prototype.Abstract &&
                prototype.TryGetComponent<StationEventComponent>(out _, componentFactory))
            .Select(prototype => prototype.ID)
            .OrderBy(id => id);
        return CompletionResult.FromHintOptions(options, "<station event>");
    }
}
