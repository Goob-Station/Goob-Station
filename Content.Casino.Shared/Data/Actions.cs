namespace Content.Casino.Shared.Data;

/// <summary>
/// Represents the result of a game action or round.
/// </summary>
public readonly record struct GameActionResult(
    bool IsComplete,
    bool Won,
    int Payout,
    string Message,
    object? GameState = null
);

/// <summary>
/// Represents a game action that can be performed by a player.
/// </summary>
public readonly record struct GameAction(
    string ActionId,
    string DisplayName,
    object? Parameters = null
);
