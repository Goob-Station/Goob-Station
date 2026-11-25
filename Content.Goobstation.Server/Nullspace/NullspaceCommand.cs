using System.Globalization;
using System.Linq;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Nullspace;

[AdminCommand(AdminFlags.Host)]
public sealed partial class NullspaceCommand : LocalizedEntityCommands
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private NullspaceSystem _nullspace = default!;

    public override string Command => "nullspace";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length > 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        ICommonSession? session;
        if (args.Length == 0 || args[0] == "_")
        {
            session = shell.Player;
        }
        else if (!_player.TryGetSessionByUsername(args[0], out session))
        {
            shell.WriteError(Loc.GetString("cmd-nullspace-no-player", ("player", args[0])));
            return;
        }

        if (session == null)
        {
            shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        float? delay = null;
        if (args.Length == 2)
        {
            if (!float.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) || seconds < 0f)
            {
                shell.WriteError(Loc.GetString("cmd-nullspace-bad-delay", ("delay", args[1])));
                return;
            }

            delay = seconds;
        }

        if (!_nullspace.TryEnter(session, delay, out var error))
        {
            shell.WriteError(Loc.GetString(error));
            return;
        }

        shell.WriteLine(Loc.GetString("cmd-nullspace-sent", ("player", session.Name)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(
                _player.Sessions.Select(s => s.Name).OrderBy(n => n),
                Loc.GetString("cmd-nullspace-hint-player")),
            2 => CompletionResult.FromHint(Loc.GetString("cmd-nullspace-hint-delay")),
            _ => CompletionResult.Empty,
        };
    }
}
