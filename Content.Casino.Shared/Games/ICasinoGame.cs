using System.Threading;
using System.Threading.Tasks;
using Content.Casino.Shared.Data;
using Robust.Shared.Player;

namespace Content.Casino.Shared.Games;

/// <summary>
/// Represents the cost and result of a game action.
/// </summary>
public readonly record struct GameActionCost(
    int Cost,
    bool RequiresPayment
);

/// <summary>
/// Base interface for all casino games.
/// Supports both simple games (slots) and complex multi-action games (blackjack).
/// </summary>
public interface ICasinoGame
{
    string GameId { get; }
    string DisplayName { get; }
    string Description { get; }
    int MinBet { get; }
    int MaxBet { get; }

    /// <summary>
    /// Initialize the game. Called once during registration.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Start a new game session for the given player.
    /// </summary>
    Task<GameSession> StartGameAsync(ICommonSession player, int initialBet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the cost of performing an action (e.g., bet amount for spinning).
    /// </summary>
    Task<GameActionCost> GetActionCostAsync(string sessionId, GameAction action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an action within an existing game session.
    /// </summary>
    Task<GameActionResult> ExecuteActionAsync(string sessionId, GameAction action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available actions for the current game state.
    /// </summary>
    Task<IReadOnlyList<GameAction>> GetAvailableActionsAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// End a game session and clean up resources.
    /// </summary>
    Task EndGameAsync(string sessionId, CancellationToken cancellationToken = default);
}
