using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Casino.Shared.Data;
using Content.Casino.Shared.Games;
using Content.Casino.Shared.Games.Slots;
using Robust.Shared.IoC;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Casino.Server.Games;

public sealed class SlotMachineGame : ICasinoGame
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly Dictionary<string, SlotGameState> _gameStates = new();

    public string GameId => "slots";
    public string DisplayName => "Slot Machine";
    public string Description => "Today's your lucky day!";
    public int MinBet => 1;
    public int MaxBet => 1000;

    public void Initialize()
    {
        IoCManager.InjectDependencies(this);
    }

    public async Task<GameSession> StartGameAsync(ICommonSession player, int initialBet, CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid().ToString();
        var session = new GameSession(sessionId, GameId, player, initialBet, DateTime.UtcNow);

        // Initialize game state
        var gameState = new SlotGameState(
            Symbols: new SlotSymbol[3],
            CurrentBet: initialBet,
            IsSpinning: false
        );

        _gameStates[sessionId] = gameState;

        return await Task.FromResult(session with { GameState = gameState.ToJson() });
    }

    public async Task<GameActionCost> GetActionCostAsync(string sessionId, GameAction action, CancellationToken cancellationToken = default)
    {
        if (!_gameStates.TryGetValue(sessionId, out var gameState))
            return new GameActionCost(0, false);

        return action.ActionId switch
        {
            "spin" => new GameActionCost(gameState.CurrentBet, true),
            "change_bet" => new GameActionCost(0, false),
            _ => new GameActionCost(0, false)
        };
    }

    public async Task<GameActionResult> ExecuteActionAsync(string sessionId, GameAction action, CancellationToken cancellationToken = default)
    {
        if (!_gameStates.TryGetValue(sessionId, out var gameState))
            throw new InvalidOperationException("Game session not found");

        return action.ActionId switch
        {
            "spin" => await ExecuteSpinAsync(sessionId, gameState, action),
            "change_bet" => await ExecuteChangeBetAsync(sessionId, gameState, action),
            _ => throw new ArgumentException($"Unknown action: {action.ActionId}")
        };
    }

    private async Task<GameActionResult> ExecuteSpinAsync(string sessionId, SlotGameState gameState, GameAction action)
    {
        if (gameState.IsSpinning)
            throw new InvalidOperationException("Already spinning!");

        // Generate random symbols
        var symbols = new SlotSymbol[3];
        for (int i = 0; i < symbols.Length; i++)
        {
            symbols[i] = (SlotSymbol)_random.Next(0, Enum.GetValues<SlotSymbol>().Length);
        }

        // Calculate payout
        var payout = symbols.GetPayout(gameState.CurrentBet);
        var won = payout > 0;

        // Update game state
        var newGameState = gameState with
        {
            Symbols = symbols,
            IsSpinning = false
        };
        _gameStates[sessionId] = newGameState;

        // Generate result message
        var symbolsDisplay = symbols.GetDisplayString();
        var winType = symbols.GetWinType();

        string message;
        if (won)
        {
            if (payout >= gameState.CurrentBet * 5)
            {
                message = $"🎉 {winType} BIG WIN! You won {payout} coins! {symbolsDisplay}";
            }
            else
            {
                message = $"🎰 {winType} You won {payout} coins! {symbolsDisplay}";
            }
        }
        else
        {
            message = $"🎰 No luck this time. You lost {gameState.CurrentBet} coins. {symbolsDisplay}";
        }

        return await Task.FromResult(new GameActionResult(
            IsComplete: false, // Slots can continue playing
            Won: won,
            Payout: payout,
            Message: message,
            GameState: newGameState.ToJson()
        ));
    }

    private async Task<GameActionResult> ExecuteChangeBetAsync(string sessionId, SlotGameState gameState, GameAction action)
    {
        if (gameState.IsSpinning)
            throw new InvalidOperationException("Cannot change bet while spinning!");

        // Parse new bet amount from action parameters
        if (!int.TryParse(action.Parameters?.ToString(), out var newBet))
            throw new ArgumentException("Invalid bet amount");

        if (newBet < MinBet || newBet > MaxBet)
            throw new ArgumentException($"Bet must be between {MinBet} and {MaxBet}");

        // Update game state
        var newGameState = gameState with { CurrentBet = newBet };
        _gameStates[sessionId] = newGameState;

        return await Task.FromResult(new GameActionResult(
            IsComplete: false,
            Won: false,
            Payout: 0,
            Message: $"💰 Bet changed to {newBet} coins",
            GameState: newGameState.ToJson()
        ));
    }

    public async Task<IReadOnlyList<GameAction>> GetAvailableActionsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (!_gameStates.TryGetValue(sessionId, out var gameState))
            return Array.Empty<GameAction>();

        var actions = new List<GameAction>();

        if (!gameState.IsSpinning)
        {
            actions.Add(new GameAction("spin", $"Spin (Bet: {gameState.CurrentBet})"));
            actions.Add(new GameAction("change_bet", "Change Bet"));
        }

        return await Task.FromResult(actions);
    }

    public async Task EndGameAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        _gameStates.Remove(sessionId);
        await Task.CompletedTask;
    }
}
