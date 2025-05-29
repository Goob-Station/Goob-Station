using System.Linq;
using System.Threading.Tasks;
using Content.Casino.Shared.Data;
using Robust.Shared.Console;

namespace Content.Casino.Client.Games.Slots;

public sealed class SlotCommand : IConsoleCommand
{
    private const int DefaultSessionBet = 5;

    [Dependency] private readonly IClientCasinoManager _casinoManager = default!;

    public string Command => "slots";
    public string Description => "Play the slot machine";
    public string Help => """
        slots - Start a slot machine session with default bet (5 coins)
        slots <bet> - Do a one-off spin with the specified bet
        slots spin - Spin the reels in current session
        slots bet <amount> - Change bet amount in current session
        slots quit - End current slot session
        slots status - Show current session status
        """;

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 0)
        {
            // No arguments yet, suggest all subcommands and common bet amounts
            var hasActiveSession = HasActiveSlotSession();

            var suggestions = new List<CompletionOption>();

            if (hasActiveSession)
            {
                // Session commands
                suggestions.Add(new CompletionOption("spin", "Spin the reels"));
                suggestions.Add(new CompletionOption("bet", "Change bet amount"));
                suggestions.Add(new CompletionOption("quit", "End session"));
                suggestions.Add(new CompletionOption("status", "Show session info"));
            }
            else
            {
                // No session, suggest starting options
                suggestions.Add(new CompletionOption("5", "One-off spin (5 coins)"));
                suggestions.Add(new CompletionOption("10", "One-off spin (10 coins)"));
                suggestions.Add(new CompletionOption("25", "One-off spin (25 coins)"));
                suggestions.Add(new CompletionOption("50", "One-off spin (50 coins)"));
                suggestions.Add(new CompletionOption("100", "One-off spin (100 coins)"));
                suggestions.Add(new CompletionOption("status", "Show session info"));
            }

            return CompletionResult.FromOptions(suggestions);
        }

        var firstArg = args[0].ToLowerInvariant();

        switch (firstArg)
        {
            case "bet":
                if (args.Length == 1)
                {
                    // Suggest common bet amounts for bet change
                    return CompletionResult.FromOptions(new[]
                    {
                        new CompletionOption("1", "Minimum bet"),
                        new CompletionOption("5", "Low bet"),
                        new CompletionOption("10", "Medium bet"),
                        new CompletionOption("25", "High bet"),
                        new CompletionOption("50", "Higher bet"),
                        new CompletionOption("100", "Maximum common bet"),
                        new CompletionOption("500", "Very high bet"),
                        new CompletionOption("1000", "Maximum bet")
                    });
                }
                break;

            case "spin":
            case "quit":
            case "end":
            case "stop":
            case "status":
            case "info":
                // These commands don't take additional parameters
                return CompletionResult.Empty;

            default:
                // For numeric inputs, suggest similar bet amounts
                if (int.TryParse(firstArg, out var currentBet))
                {
                    var suggestions = new List<CompletionOption>();
                    var baseBets = new[] { 1, 5, 10, 25, 50, 100, 250, 500, 1000 };

                    foreach (var bet in baseBets)
                    {
                        if (bet.ToString().StartsWith(firstArg))
                        {
                            suggestions.Add(new CompletionOption(bet.ToString(), $"One-off spin ({bet} coins)"));
                        }
                    }

                    return CompletionResult.FromOptions(suggestions);
                }
                break;
        }

        return CompletionResult.Empty;
    }

    private bool HasActiveSlotSession()
    {
        return _casinoManager.ActiveSessions.Values.Any(s => s.GameId == "slots");
    }

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        IoCManager.InjectDependencies(this);

        if (args.Length == 0)
        {
            // Start session with default bet
            ExecuteSessionStart(shell, DefaultSessionBet);
            return;
        }

        var firstArg = args[0].ToLowerInvariant();

        switch (firstArg)
        {
            case "spin":
                ExecuteSpinCommand(shell);
                break;

            case "bet":
                ExecuteBetCommand(shell, args);
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
                // Try to parse as bet amount for one-off spin
                if (int.TryParse(firstArg, out var bet))
                {
                    ExecuteOneOffSpin(shell, bet);
                }
                else
                {
                    shell.WriteError($"Unknown subcommand '{firstArg}'. Use 'help slots' for usage.");
                }
                break;
        }
    }

    private void ExecuteOneOffSpin(IConsoleShell shell, int bet)
    {
        if (bet <= 0)
        {
            shell.WriteError("Bet must be a positive number.");
            return;
        }

        if (bet > 1000)
        {
            shell.WriteError("Bet must be between 1 and 1000 coins.");
            return;
        }

        // Check if there's already an active session
        var existingSession = _casinoManager.ActiveSessions.Values
            .FirstOrDefault(s => s.GameId == "slots");

        if (existingSession != null)
        {
            shell.WriteError("You have an active slot session. Use 'slots spin' to play or 'slots quit' to end the session first.");
            return;
        }

        // Don't show verbose startup message for one-off spins
        // The result will speak for itself

        // Use Task.Run to handle async in sync context properly
        Task.Run(async () =>
        {
            try
            {
                // Start session
                await _casinoManager.StartGameAsync("slots", bet);

                // Wait a moment for session to be registered
                await Task.Delay(100);

                // Find the session that was just created
                var session = _casinoManager.ActiveSessions.Values
                    .FirstOrDefault(s => s.GameId == "slots");

                if (session != null)
                {
                    // Spin immediately
                    var spinAction = new GameAction("spin", "Spin");
                    await _casinoManager.ExecuteActionAsync(session.SessionId, spinAction);

                    // Wait for spin result
                    await Task.Delay(100);

                    // End session immediately after spin
                    await _casinoManager.EndGameAsync(session.SessionId);
                }
            }
            catch (Exception ex)
            {
                // Log the error so we can see what's happening
                shell.WriteError($"One-off spin failed: {ex.Message}");
            }
        });
    }

    private void ExecuteSessionStart(IConsoleShell shell, int bet)
    {
        if (bet <= 0)
        {
            shell.WriteError("Bet must be a positive number.");
            return;
        }

        if (bet > 1000)
        {
            shell.WriteError("Bet must be between 1 and 1000 coins.");
            return;
        }

        // Check if there's already an active session
        var existingSession = _casinoManager.ActiveSessions.Values
            .FirstOrDefault(s => s.GameId == "slots");

        if (existingSession != null)
        {
            shell.WriteError("You already have an active slot session. Use 'slots quit' to end it first.");
            return;
        }

        shell.WriteLine($"🎰 Starting slot machine session with {bet} coin bet...");

        // Use Task.Run to properly handle async operation
        Task.Run(async () =>
        {
            try
            {
                await _casinoManager.StartGameAsync("slots", bet);
            }
            catch (Exception ex)
            {
                shell.WriteError($"Failed to start session: {ex.Message}");
            }
        });
    }

    private void ExecuteSpinCommand(IConsoleShell shell)
    {
        var activeSession = _casinoManager.ActiveSessions.Values
            .FirstOrDefault(s => s.GameId == "slots");

        if (activeSession == null)
        {
            shell.WriteError("No active slot session. Use 'slots' to start a session or 'slots <bet>' for a one-off spin.");
            return;
        }

        var spinAction = new GameAction("spin", "Spin");
        _ = _casinoManager.ExecuteActionAsync(activeSession.SessionId, spinAction);
        shell.WriteLine("🎰 Spinning the reels...");
    }

    private void ExecuteBetCommand(IConsoleShell shell, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError("Usage: slots bet <amount>");
            return;
        }

        if (!int.TryParse(args[1], out var newBet) || newBet <= 0)
        {
            shell.WriteError("Bet amount must be a positive number.");
            return;
        }

        var activeSession = _casinoManager.ActiveSessions.Values
            .FirstOrDefault(s => s.GameId == "slots");

        if (activeSession == null)
        {
            shell.WriteError("No active slot session. Use 'slots' to start a session.");
            return;
        }

        if (newBet > 1000)
        {
            shell.WriteError("Bet must be between 1 and 1000 coins.");
            return;
        }

        var changeBetAction = new GameAction("change_bet", "Change Bet", newBet);
        _ = _casinoManager.ExecuteActionAsync(activeSession.SessionId, changeBetAction);
        shell.WriteLine($"💰 Changing bet to {newBet} coins...");
    }

    private void ExecuteQuitCommand(IConsoleShell shell)
    {
        var activeSession = _casinoManager.ActiveSessions.Values
            .FirstOrDefault(s => s.GameId == "slots");

        if (activeSession == null)
        {
            shell.WriteError("No active slot session.");
            return;
        }

        _ = _casinoManager.EndGameAsync(activeSession.SessionId);
        shell.WriteLine("🎰 Ending slot machine session...");
    }

    private void ExecuteStatusCommand(IConsoleShell shell)
    {
        var activeSession = _casinoManager.ActiveSessions.Values
            .FirstOrDefault(s => s.GameId == "slots");

        if (activeSession == null)
        {
            shell.WriteLine("No active slot machine session.");
            shell.WriteLine("Use 'slots' to start a session or 'slots <bet>' for a one-off spin!");
            return;
        }

        shell.WriteLine("Active Slot Machine Session:");
        shell.WriteLine($"  Session ID: {activeSession.SessionId[..8]}...");
        shell.WriteLine($"  Started: {activeSession.StartTime:HH:mm:ss}");
        shell.WriteLine($"  Initial Bet: {activeSession.InitialBet} coins");
        shell.WriteLine("Use 'slots spin' to play or 'slots quit' to end session.");
    }
}
