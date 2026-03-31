using Content.Server._CorvaxGoob.Photo;
using Content.Server.Administration;
using Content.Shared._CorvaxGoob.Photo;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._CorvaxGoob.Administration.Commands;

[AdminCommand(AdminFlags.Moderator)]
public sealed class CaptureClydeCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _manager = default!;

    public string Command => "capturescreenclyde";
    public string Description => "Capture screen of target via Clyde renderer. If you see black screen, use capturescreenviewport instead.";
    public string Help => "capturescreenclyde <username>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is null)
        {
            shell.WriteLine(Loc.GetString("shell-only-players-can-run-this-command"));
            return;
        }

        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
        {
            shell.WriteLine(Loc.GetString("shell-target-player-does-not-exist"));
            return;
        }

        if (!_player.TryGetSessionByUsername(args[0], out var session))
        {
            shell.WriteLine(Loc.GetString("shell-target-player-does-not-exist"));
            return;
        }

        _manager.System<CaptureSystem>().RequestScreenCapture(session, shell.Player, PhotoCaptureType.Clyde);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(),Loc.GetString("shell-argument-username-hint"));
        }
        return CompletionResult.Empty;
    }
}
