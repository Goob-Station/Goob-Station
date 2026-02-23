using Content.Goobstation.Common.AntagToken;
using Content.Goobstation.Common.CCVar;
using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Database;
using Content.Shared.Administration;
using Content.Shared.Database;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

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

        var adminLog = IoCManager.Resolve<IAdminLogManager>();
        adminLog.Add(LogType.AntagToken, LogImpact.High, $"Admin {shell.Player?.Name ?? "CONSOLE"} deactivated antag token for player '{args[0]}'.");

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

[AdminCommand(AdminFlags.Admin)]
public sealed class GrantAntagTokenCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public string Command => "grantantagtoken";
    public string Description => "Grants antag token(s) to a player.";
    public string Help => "Usage: grantantagtoken <username> [amount]";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1 || args.Length > 2)
        {
            shell.WriteError("Expected one or two arguments (player name, optional amount).");
            return;
        }

        if (!_playerManager.TryGetUserId(args[0], out var userId))
        {
            shell.WriteError($"Unable to find player '{args[0]}'.");
            return;
        }

        var amount = 1;
        if (args.Length == 2 && (!int.TryParse(args[1], out amount) || amount <= 0))
        {
            shell.WriteError("Amount must be a positive integer.");
            return;
        }

        var db = IoCManager.Resolve<IServerDbManager>();
        var cfg = IoCManager.Resolve<IConfigurationManager>();
        var cap = cfg.GetCVar(GoobCVars.AntagTokenCap);

        var newCount = await db.IncrementAntagTokens(userId, amount, cap);

        var adminLog = IoCManager.Resolve<IAdminLogManager>();
        adminLog.Add(LogType.AntagToken, LogImpact.High, $"Admin {shell.Player?.Name ?? "CONSOLE"} granted {amount} antag token(s) to player '{args[0]}'. New total: {newCount}.");

        shell.WriteLine($"Granted {amount} antag token(s) to '{args[0]}'. They now have {newCount}.");
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "Player name"),
            2 => CompletionResult.FromHint("Amount (default: 1)"),
            _ => CompletionResult.Empty
        };
    }
}
