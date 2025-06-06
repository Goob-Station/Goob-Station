using System.Threading.Tasks;
using Content.Casino.Shared.Data;

namespace Content.Casino.Shared.Games;

/// <summary>
/// Client-side interface for game UI implementations.
/// </summary>
public interface IGameClientHandler
{
    string GameId { get; }

    /// <summary>
    /// Initialize the client handler. Called once during registration.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Handle the start of a new game session.
    /// </summary>
    Task OnGameStartedAsync(GameSession session);

    /// <summary>
    /// Handle the result of a game action.
    /// </summary>
    Task OnActionResultAsync(string sessionId, GameAction action, GameActionResult result);

    /// <summary>
    /// Handle available actions being updated.
    /// </summary>
    Task OnActionsUpdatedAsync(string sessionId, IReadOnlyList<GameAction> actions);

    /// <summary>
    /// Handle the end of a game session.
    /// </summary>
    Task OnGameEndedAsync(string sessionId, GameActionResult finalResult);

    /// <summary>
    /// Show the game UI to the player.
    /// </summary>
    Task ShowGameUIAsync();

    /// <summary>
    /// Hide the game UI.
    /// </summary>
    Task HideGameUIAsync();
}
