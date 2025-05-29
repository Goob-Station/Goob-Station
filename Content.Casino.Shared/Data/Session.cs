using Robust.Shared.Player;

namespace Content.Casino.Shared.Data;

/// <summary>
/// Represents an active game session.
/// </summary>
public sealed record GameSession(
    string SessionId,
    string GameId,
    ICommonSession Player,
    int InitialBet,
    DateTime StartTime,
    object? GameState = null
);
