namespace Content.Casino.Shared.Data;

/// <summary>
/// Gives a result for a played game
/// </summary>
/// <param name="Won">Did the player win</param>
/// <param name="Payout">Payout to be given</param>
/// <param name="Message">Message to send to the client</param>
/// <param name="GameStateId">ID for the game, if null - it's over.</param>
public record struct GameResult(
    bool Won,
    int Payout,
    string Message,
    string? GameStateId = null
);
