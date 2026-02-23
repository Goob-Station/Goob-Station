using System.Linq;
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
    [Dependency] private readonly IAntagTokenManager _antagToken = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;

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

        if (!_antagToken.HasActiveToken(userId))
        {
            shell.WriteLine($"Player '{args[0]}' does not have an active antag token.");
            return;
        }

        _antagToken.DeactivateToken(userId);
        _adminLog.Add(LogType.AntagToken, LogImpact.High, $"Admin {shell.Player?.Name ?? "CONSOLE"} deactivated antag token for player '{args[0]}'.");

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
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;

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

        var cap = _cfg.GetCVar(GoobCVars.AntagTokenCap);
        var newCount = await _db.IncrementAntagTokens(userId, amount, cap);
        _adminLog.Add(LogType.AntagToken, LogImpact.High, $"Admin {shell.Player?.Name ?? "CONSOLE"} granted {amount} antag token(s) to player '{args[0]}'. New total: {newCount}.");

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

[AdminCommand(AdminFlags.Admin)]
public sealed class ListAntagTokensCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IAntagTokenManager _antagToken = default!;

    public string Command => "listantagtokens";
    public string Description => "Lists all players with an active antag token.";
    public string Help => "Usage: listantagtokens";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var active = _antagToken.GetActiveTokenUsers();
        if (active.Count == 0)
        {
            shell.WriteLine("No players have active antag tokens.");
            return;
        }

        var names = active.Select(id =>
            _playerManager.TryGetSessionById(id, out var s) ? s.Name : id.ToString());
        shell.WriteLine($"Active antag tokens ({active.Count}): {string.Join(", ", names)}");
    }
}
