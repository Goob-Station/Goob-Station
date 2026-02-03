using Content.Goobstation.Common.AntagToken;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.IoC;

namespace Content.Goobstation.Server.AntagToken;

[AdminCommand(AdminFlags.Admin)]
public sealed class DeactivateAntagTokenCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public string Command => "deactivateantagtoken";
    public string Description => "Deactivates a player's antag token so they won't get a weight boost for antag selection.";
    public string Help => "Usage: deactivateantagtoken <username>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError("Expected exactly one argument (player name).");
            return;
        }

        if (!_playerManager.TryGetUserId(args[0], out var userId))
        {
            shell.WriteError($"Unable to find player '{args[0]}'.");
            return;
        }

        var tokenManager = IoCManager.Resolve<IAntagTokenManager>();

        if (!tokenManager.HasActiveToken(userId))
        {
            shell.WriteLine($"Player '{args[0]}' does not have an active antag token.");
            return;
        }

        tokenManager.DeactivateToken(userId);
        shell.WriteLine($"Deactivated antag token for player '{args[0]}'.");
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "Player name"),
            _ => CompletionResult.Empty
        };
    }
}
