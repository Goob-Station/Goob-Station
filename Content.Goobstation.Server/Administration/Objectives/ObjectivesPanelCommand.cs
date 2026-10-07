using System.Linq;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Goobstation.Server.Administration.Objectives;

/// <summary>
/// Opens the objectives panel for a player, a UI alternative to <c>addobjective</c>, <c>rmobjective</c> and <c>lsobjectives</c>.
/// </summary>
[AdminCommand(AdminFlags.Admin)]
public sealed class ObjectivesPanelCommand : LocalizedEntityCommands
{
    [Dependency] private readonly AdminObjectivesSystem _adminObjectives = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override string Command => "objectivespanel";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } admin)
        {
            shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific", ("properAmount", 1), ("currentAmount", args.Length)));
            return;
        }

        if (!_players.TryGetSessionByUsername(args[0], out var session))
        {
            shell.WriteError(Loc.GetString("cmd-objectivespanel-player-not-found"));
            return;
        }

        if (!_mind.TryGetMind(session, out var mindId, out _))
        {
            shell.WriteError(Loc.GetString("cmd-objectivespanel-mind-not-found"));
            return;
        }

        _adminObjectives.OpenPanel(admin, mindId);
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length != 1)
            return CompletionResult.Empty;

        var options = _players.Sessions.OrderBy(s => s.Name).Select(s => s.Name).ToArray();
        return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-objectivespanel-player-completion"));
    }
}
