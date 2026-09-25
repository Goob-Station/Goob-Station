using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Goobstation.Server.VoiceChat;

[AdminCommand(AdminFlags.Logs)]
public sealed class VoiceLogsCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly VoiceLogSystem _voiceLogs = default!;

    public override string Command => "voicelogs";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } admin)
        {
            shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        Guid? user = null;
        if (args.Length > 0)
        {
            if (!_player.TryGetSessionByUsername(args[0], out var session))
            {
                shell.WriteError(Loc.GetString("shell-target-player-does-not-exist"));
                return;
            }

            user = session.UserId.UserId;
        }

        _voiceLogs.OpenViewer(admin, user);
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length == 1
            ? CompletionResult.FromHintOptions(CompletionHelper.SessionNames(players: _player), Loc.GetString("cmd-voicelogs-hint"))
            : CompletionResult.Empty;
    }
}
