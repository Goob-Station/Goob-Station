using Content.Server._CorvaxGoob.Photo;
using Content.Server.Administration;
using Content.Shared._CorvaxGoob.Photo;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._CorvaxGoob.Administration.Commands;

[AdminCommand(AdminFlags.Moderator)]
public sealed class CaptureViewportCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _manager = default!;

    public string Command => "capturescreenviewport";
    public string Description => "Capture screen of target via third party renderers.";
    public string Help => "capturescreenviewport <username>";

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

        _manager.System<CaptureSystem>().RequestScreenCapture(session, shell.Player, PhotoCaptureType.Viewport);
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
