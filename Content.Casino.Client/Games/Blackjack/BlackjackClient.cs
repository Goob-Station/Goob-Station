using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Content.Casino.Shared.Data;
using Content.Casino.Shared.Games;
using Robust.Client.Console;
using Robust.Shared.IoC;

namespace Content.Casino.Client.Games.Blackjack;

public sealed class BlackjackClientHandler : IGameClientHandler
{
    [Dependency] private readonly IClientConsoleHost _consoleHost = default!;

    public string GameId => "blackjack";

    private string? _currentSessionId;
    private int _currentBet = 0;
    private int _gamesPlayed = 0;
    private int _gamesWon = 0;
    private int _totalWinnings = 0;

    public void Initialize()
    {
        IoCManager.InjectDependencies(this);
    }

    public async Task OnGameStartedAsync(GameSession session)
    {
        _currentSessionId = session.SessionId;
        _currentBet = session.InitialBet;

        _consoleHost.WriteLine(null, "=== BLACKJACK TABLE ===");
        _consoleHost.WriteLine(null, $"Starting new blackjack game with {_currentBet} coin bet");
        _consoleHost.WriteLine(null, "Goal: Get as close to 21 as possible without going over");
        _consoleHost.WriteLine(null, "Dealer stands on 17, hits on 16");
        _consoleHost.WriteLine(null, "Blackjack pays 3:2");
        _consoleHost.WriteLine(null, "");

        // Display initial deal if available in serialized game state
        if (!string.IsNullOrEmpty(session.GameState?.ToString()))
        {
            _consoleHost.WriteLine(null, session.GameState.ToString()!);
            _consoleHost.WriteLine(null, "");
        }

        await Task.CompletedTask;
    }

    public async Task OnActionResultAsync(string sessionId, GameAction action, GameActionResult result)
    {
        if (sessionId != _currentSessionId)
            return;

        if (action.ActionId != "status")
        {
            _consoleHost.WriteLine(null, result.Message);
        }

        // Track game completion
        if (result.IsComplete)
        {
            _gamesPlayed++;
            if (result.Won || result.Payout > 0)
            {
                _gamesWon++;
                _totalWinnings += result.Payout;
            }
        }

        await Task.CompletedTask;
    }

    public async Task OnActionsUpdatedAsync(string sessionId, IReadOnlyList<GameAction> actions)
    {
        if (sessionId != _currentSessionId)
            return;

        if (actions.Count > 0)
        {
            _consoleHost.WriteLine(null, "Available actions:");
            foreach (var action in actions)
            {
                var commandHint = action.ActionId switch
                {
                    "hit" => "'bj hit'",
                    "stand" => "'bj stand'",
                    "double" => "'bj double'",
                    "split" => "'bj split'",
                    "insurance" => "'bj insurance'",
                    _ => $"'blackjack {action.ActionId}'"
                };
                _consoleHost.WriteLine(null, $"  {action.DisplayName} - {commandHint}");
            }
        }

        await Task.CompletedTask;
    }

    public async Task OnGameEndedAsync(string sessionId, GameActionResult finalResult)
    {
        if (sessionId != _currentSessionId)
            return;

        // Reset session state
        _currentSessionId = null;
        _currentBet = 0;

        await Task.CompletedTask;
    }

    public async Task ShowGameUIAsync()
    {
        _consoleHost.WriteLine(null, "Blackjack Commands:");
        _consoleHost.WriteLine(null, "  bj [bet] - Start game");
        _consoleHost.WriteLine(null, "  bj hit/stand/double/split/insurance - Game actions");
        _consoleHost.WriteLine(null, "  bj quit - End game");

        await Task.CompletedTask;
    }

    public async Task HideGameUIAsync()
    {
        // Nothing to hide in console
        await Task.CompletedTask;
    }
}
