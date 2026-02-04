using Content.Server._CorvaxGoob.Photo;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._CorvaxGoob.Administration.Commands;

[AdminCommand(AdminFlags.Moderator)]
public sealed class CaptureCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _manager = default!;

    public string Command => "capturescreen";
    public string Description => Loc.GetString("cmd-capturescreen-desc");
    public string Help => Loc.GetString("cmd-capturescreen-help", ("command", Command));

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is null)
        {
            shell.WriteLine(Loc.GetString("shell-only-players-can-run-this-command"));
            return;
        }

        if (args[0] is null)
        {
            shell.WriteLine(Loc.GetString("shell-target-player-does-not-exist"));
            return;
        }

        if (!_player.TryGetSessionByUsername(args[0], out var session))
        {
            shell.WriteLine(Loc.GetString("shell-target-player-does-not-exist"));
            return;
        }

        _manager.System<CaptureSystem>().RequestScreenCapture(session, shell.Player);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("shell-argument-username-hint"));
        }

        return CompletionResult.Empty;
    }
}
