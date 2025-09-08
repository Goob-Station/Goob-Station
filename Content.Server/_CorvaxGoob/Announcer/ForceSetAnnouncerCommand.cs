using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Server._CorvaxGoob.Announcer;

[AdminCommand(AdminFlags.Fun)]
public sealed class ForceSetAnnouncerCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public string Command => "forcesetannouncer";
    public string Description => Loc.GetString("force-set-announcer-command-description", ("command", Command));
    public string Help => Loc.GetString("force-set-announcer-command-help-text", ("command", Command));

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments", ("lower", 1), ("upper", 2), ("currentAmount", args.Length)));
            return;
        }

        var announcerSystem = _entity.System<AnnouncerSystem>();

        var rounds = 1;

        if (!_prototype.TryIndex<AnnouncerPrototype>(args[0], out var prototype))
            return;

        if (args.Length == 2 && !int.TryParse(args[1], out rounds))
        {
            shell.WriteError(Loc.GetString("force-set-announcer-optional-argument-not-integer"));
            return;
        }

        announcerSystem.ForceSetAnnouncer(prototype, rounds);
        shell.WriteLine(Loc.GetString("force-set-announcer-set-finite", ("announcer", prototype.ID), ("rounds", rounds.ToString())));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var gamePresets = _prototype.EnumeratePrototypes<AnnouncerPrototype>()
                .OrderBy(p => p.ID);
            var options = new List<string>();
            foreach (var preset in gamePresets)
            {
                options.Add(preset.ID);
            }

            return CompletionResult.FromHintOptions(options, "<id>");
        }
        else if (args.Length == 2)
            return CompletionResult.FromHint("<rounds count>");
        return CompletionResult.Empty;
    }
}
