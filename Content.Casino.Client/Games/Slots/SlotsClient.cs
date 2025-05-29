using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Content.Casino.Shared.Data;
using Content.Casino.Shared.Games;
using Content.Casino.Shared.Games.Slots;
using Robust.Client.Console;
using Robust.Shared.IoC;

namespace Content.Casino.Client.Games.Slots;

public sealed class SlotMachineClientHandler : IGameClientHandler
{
    [Dependency] private readonly IClientConsoleHost _consoleHost = default!;

    public string GameId => "slots";

    private string? _currentSessionId;
    private SlotGameState? _currentGameState;
    private int _wins = 0;
    private int _losses = 0;

    public void Initialize()
    {
        IoCManager.InjectDependencies(this);
    }

    public async Task OnGameStartedAsync(GameSession session)
    {
        _currentSessionId = session.SessionId;

        if (session.GameState != null)
        {
            _currentGameState = SlotGameState.FromJson(session.GameState.ToString()!);
        }

        _consoleHost.WriteLine(null, "🎰 Slot machine session started!");
        _consoleHost.WriteLine(null, $"Initial bet: {session.InitialBet} coins");
        _consoleHost.WriteLine(null, "Use 'slots spin' to play, 'slots bet <amount>' to change bet, 'slots quit' to end session.");

        await Task.CompletedTask;
    }

    public async Task OnActionResultAsync(string sessionId, GameAction action, GameActionResult result)
    {
        if (sessionId != _currentSessionId)
            return;

        // Update game state
        if (result.GameState != null)
        {
            _currentGameState = SlotGameState.FromJson(result.GameState.ToString()!);
        }

        // Display the result message
        _consoleHost.WriteLine(null, result.Message);

        // Track wins/losses for spin actions
        if (action.ActionId == "spin")
        {
            if (result.Won)
            {
                _wins++;
                if (result.Payout >= (_currentGameState?.CurrentBet ?? 0) * 5)
                {
                    _consoleHost.WriteLine(null, "🎉 BIG WIN! 🎉");
                }
            }
            else
            {
                _losses++;
            }

            // Show stats periodically
            var totalGames = _wins + _losses;
            if (totalGames % 5 == 0 && totalGames > 0)
            {
                var winRate = (double)_wins / totalGames * 100;
                _consoleHost.WriteLine(null, $"📊 Stats: {_wins} wins, {_losses} losses ({winRate:F1}% win rate)");
            }
        }

        await Task.CompletedTask;
    }

    public async Task OnActionsUpdatedAsync(string sessionId, IReadOnlyList<GameAction> actions)
    {
        // Console doesn't need real-time action updates
        await Task.CompletedTask;
    }

    public async Task OnGameEndedAsync(string sessionId, GameActionResult finalResult)
    {
        if (sessionId != _currentSessionId)
            return;

        _consoleHost.WriteLine(null, "🎰 Slot machine session ended.");
        if (_wins > 0 || _losses > 0)
        {
            var totalGames = _wins + _losses;
            var winRate = totalGames > 0 ? (double)_wins / totalGames * 100 : 0;
            _consoleHost.WriteLine(null, $"Final stats: {_wins} wins, {_losses} losses ({winRate:F1}% win rate)");
        }

        _currentSessionId = null;
        _currentGameState = null;
        _wins = 0;
        _losses = 0;

        await Task.CompletedTask;
    }

    public async Task ShowGameUIAsync()
    {
        _consoleHost.WriteLine(null, "🎰 Slot machine console interface active.");
        _consoleHost.WriteLine(null, "Commands: 'slots <bet>' to start, 'slots spin', 'slots bet <amount>', 'slots quit'");
        await Task.CompletedTask;
    }

    public async Task HideGameUIAsync()
    {
        // Nothing to hide in console
        await Task.CompletedTask;
    }
}
