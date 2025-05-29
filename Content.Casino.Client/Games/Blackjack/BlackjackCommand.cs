using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Casino.Shared.Data;
using Robust.Shared.Console;

namespace Content.Casino.Client.Games.Blackjack;

public sealed class BlackjackCommand : IConsoleCommand
{
    private const int DefaultBet = 10;

    [Dependency] private readonly IClientCasinoManager _casinoManager = default!;

    public string Command => "bj";
    public string Description => "Play blackjack";
    public string Help => """
        bj - Start a blackjack game with default bet (10 coins)
        bj <bet> - Start a blackjack game with specified bet
        bj hit - Take another card
        bj stand - Keep current hand value
        bj double - Double your bet and take exactly one more card
        bj split - Split a pair into two separate hands
        bj insurance - Take insurance bet against dealer blackjack
        bj quit - End current game
        bj status - Show current game status
        """;

    public async ValueTask<CompletionResult> GetCompletionAsync(IConsoleShell shell, string[] args, string argStr, CancellationToken cancel)
    {
        if (args.Length == 1)
        {
            var hasActiveSession = HasActiveBlackjackSession();
            var suggestions = new List<CompletionOption>();

            if (hasActiveSession)
            {
                // Game action commands
                suggestions.Add(new CompletionOption("hit", "Take another card"));
                suggestions.Add(new CompletionOption("stand", "Keep current hand"));
                suggestions.Add(new CompletionOption("double", "Double bet and take one card"));
                suggestions.Add(new CompletionOption("split", "Split matching pair"));
                suggestions.Add(new CompletionOption("insurance", "Take insurance bet"));
                suggestions.Add(new CompletionOption("quit", "End game"));
                suggestions.Add(new CompletionOption("status", "Show game status"));
            }
            else
            {
                // Start game options
                suggestions.Add(new CompletionOption("5", "Start game (5 coins)"));
                suggestions.Add(new CompletionOption("10", "Start game (10 coins)"));
                suggestions.Add(new CompletionOption("25", "Start game (25 coins)"));
                suggestions.Add(new CompletionOption("50", "Start game (50 coins)"));
                suggestions.Add(new CompletionOption("100", "Start game (100 coins)"));
                suggestions.Add(new CompletionOption("status", "Show game status"));
            }

            return CompletionResult.FromOptions(suggestions);
        }

        var firstArg = args[1].ToLowerInvariant();

        switch (firstArg)
        {
            case "hit":
            case "stand":
            case "double":
            case "split":
            case "insurance":
            case "quit":
            case "end":
            case "stop":
            case "status":
            case "info":
                return CompletionResult.Empty;

            default:
                // For numeric inputs, suggest similar bet amounts
                if (int.TryParse(firstArg, out var currentBet))
                {
                    var suggestions = new List<CompletionOption>();
                    var baseBets = new[] { 5, 10, 25, 50, 100, 250, 500 };

                    foreach (var bet in baseBets)
                    {
                        if (bet.ToString().StartsWith(firstArg))
                        {
                            suggestions.Add(new CompletionOption(bet.ToString(), $"Start game ({bet} coins)"));
                        }
                    }

                    return CompletionResult.FromOptions(suggestions);
                }
                break;
        }

        return await ValueTask.FromResult(CompletionResult.Empty);
    }

    private bool HasActiveBlackjackSession()
    {
        try
        {
            return _casinoManager?.ActiveSessions?.Values?.Any(s => s.GameId == "blackjack") ?? false;
        }
        catch
        {
            return false;
        }
    }

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        IoCManager.InjectDependencies(this);

        if (args.Length == 0)
        {
            // Start game with default bet
            ExecuteStartGame(shell, DefaultBet);
            return;
        }

        var firstArg = args[0].ToLowerInvariant();

        switch (firstArg)
        {
            case "hit":
                ExecuteGameAction(shell, "hit");
                break;

            case "stand":
                ExecuteGameAction(shell, "stand");
                break;

            case "double":
            case "doubledown":
                ExecuteGameAction(shell, "double");
                break;

            case "split":
                ExecuteGameAction(shell, "split");
                break;

            case "insurance":
                ExecuteGameAction(shell, "insurance");
                break;

            case "quit":
            case "end":
            case "stop":
                ExecuteQuitCommand(shell);
                break;

            case "status":
            case "info":
                ExecuteStatusCommand(shell);
                break;

            default:
                // Try to parse as bet amount
                if (int.TryParse(firstArg, out var bet))
                {
                    ExecuteStartGame(shell, bet);
                }
                else
                {
                    shell.WriteError($"Unknown subcommand '{firstArg}'. Use 'help bj' for usage.");
                }
                break;
        }
    }

    private void ExecuteStartGame(IConsoleShell shell, int bet)
    {
        if (bet < 5)
        {
            shell.WriteError("Minimum bet is 5 coins.");
            return;
        }

        if (bet > 500)
        {
            shell.WriteError("Maximum bet is 500 coins.");
            return;
        }

        // Check if there's already an active session
        var existingSession = _casinoManager.ActiveSessions.Values
            .FirstOrDefault(s => s.GameId == "blackjack");

        if (existingSession != null)
        {
            shell.WriteError("You already have an active blackjack game. Use 'bj quit' to end it first.");
            return;
        }

        shell.WriteLine($"Starting blackjack game with {bet} coin bet...");

        Task.Run(async () =>
        {
            try
            {
                await _casinoManager.StartGameAsync("blackjack", bet);
            }
            catch (Exception ex)
            {
                shell.WriteError($"Failed to start game: {ex.Message}");
            }
        });
    }

    private void ExecuteGameAction(IConsoleShell shell, string actionId)
    {
        var activeSession = _casinoManager.ActiveSessions.Values
            .FirstOrDefault(s => s.GameId == "blackjack");

        if (activeSession == null)
        {
            shell.WriteError("No active blackjack game. Use 'bj' to start a new game.");
            return;
        }

        var action = new GameAction(actionId, actionId switch
        {
            "hit" => "Hit",
            "stand" => "Stand",
            "double" => "Double Down",
            "split" => "Split",
            "insurance" => "Insurance",
            _ => actionId
        });

        var actionName = actionId switch
        {
            "hit" => "Taking card...",
            "stand" => "Standing...",
            "double" => "Doubling down...",
            "split" => "Splitting hand...",
            "insurance" => "Taking insurance...",
            _ => $"Executing {actionId}..."
        };

        _ = _casinoManager.ExecuteActionAsync(activeSession.SessionId, action);
        shell.WriteLine(actionName);
    }

    private void ExecuteQuitCommand(IConsoleShell shell)
    {
        var activeSession = _casinoManager.ActiveSessions.Values
            .FirstOrDefault(s => s.GameId == "blackjack");

        if (activeSession == null)
        {
            shell.WriteError("No active blackjack game.");
            return;
        }

        _ = _casinoManager.EndGameAsync(activeSession.SessionId);
        shell.WriteLine("Ending blackjack game...");
    }

    private void ExecuteStatusCommand(IConsoleShell shell)
    {
        var activeSession = _casinoManager.ActiveSessions.Values
            .FirstOrDefault(s => s.GameId == "blackjack");

        if (activeSession == null)
        {
            shell.WriteLine("No active blackjack game.");
            shell.WriteLine("Use 'bj' to start a new game!");
            return;
        }

        shell.WriteLine("Active Blackjack Game:");
        shell.WriteLine($"  Session ID: {activeSession.SessionId[..8]}...");
        shell.WriteLine($"  Started: {activeSession.StartTime:HH:mm:ss}");
        shell.WriteLine($"  Initial Bet: {activeSession.InitialBet} coins");
        shell.WriteLine("Use game action commands or 'bj quit' to end game.");
    }
}
