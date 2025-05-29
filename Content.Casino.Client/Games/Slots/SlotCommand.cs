using System.Linq;
using Content.Casino.Shared.Data;
using Robust.Shared.Console;
using Robust.Shared.IoC;

namespace Content.Casino.Client.Commands;

public sealed class SlotCommand : IConsoleCommand
{
    [Dependency] private readonly IClientCasinoManager _casinoManager = default!;

    public string Command => "slots";
    public string Description => "Play the slot machine";
    public string Help => """
        slots <bet> - Start a slot machine game with the specified bet
        slots spin - Spin the reels in current session
        slots bet <amount> - Change bet amount in current session
        slots quit - End current slot session
        slots status - Show current session status
        """;

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        IoCManager.InjectDependencies(this);

        if (args.Length == 0)
        {
            shell.WriteError("Usage: slots <bet> | slots spin | slots bet <amount> | slots quit | slots status");
            shell.WriteLine("Examples:");
            shell.WriteLine("  slots 50     - Start with 50 coin bet");
            shell.WriteLine("  slots spin   - Spin the reels");
            shell.WriteLine("  slots quit   - End session");
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
                // Try to parse as bet amount for quick start
                if (int.TryParse(firstArg, out var bet))
                {
                    ExecuteQuickStart(shell, bet);
                }
                else
                {
                    shell.WriteError($"Unknown subcommand '{firstArg}'. Use 'help slots' for usage.");
                }
                break;
        }
    }

    private void ExecuteSpinCommand(IConsoleShell shell)
    {
        var activeSession = _casinoManager.ActiveSessions.Values
            .FirstOrDefault(s => s.GameId == "slots");

        if (activeSession == null)
        {
            shell.WriteError("No active slot session. Use 'slots <bet>' to start a game.");
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
            shell.WriteError("No active slot session. Use 'slots <bet>' to start a game.");
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
            shell.WriteLine("Use 'slots <bet>' to start playing!");
            return;
        }

        shell.WriteLine("🎰 Active Slot Machine Session:");
        shell.WriteLine($"  Session ID: {activeSession.SessionId[..8]}...");
        shell.WriteLine($"  Started: {activeSession.StartTime:HH:mm:ss}");
        shell.WriteLine($"  Initial Bet: {activeSession.InitialBet} coins");
        shell.WriteLine("Use 'slots spin' to play or 'slots quit' to end session.");
    }

    private void ExecuteQuickStart(IConsoleShell shell, int bet)
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

        _ = _casinoManager.StartGameAsync("slots", bet);
        shell.WriteLine($"🎰 Starting slot machine with {bet} coin bet...");
    }
}
