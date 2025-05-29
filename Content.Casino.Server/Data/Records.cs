using Content.Casino.Shared.Data;

namespace Content.Casino.Server.Data;

/// <summary>
/// Represents the result of starting a casino game session.
/// </summary>
public sealed record StartGameResult(
    bool Success,
    GameSession? Session,
    string ErrorMessage)
{
    /// <summary>
    /// Creates a successful start game result.
    /// </summary>
    public static StartGameResult Successful(GameSession session)
        => new(true, session, string.Empty);

    /// <summary>
    /// Creates a failed start game result.
    /// </summary>
    public static StartGameResult Failed(string errorMessage)
        => new(false, null, errorMessage);
}

/// <summary>
/// Represents the result of executing a game action.
/// </summary>
public sealed record ExecuteActionResult(
    bool Success,
    GameActionResult Result,
    string ErrorMessage)
{
    /// <summary>
    /// Creates a successful execute action result.
    /// </summary>
    public static ExecuteActionResult Successful(GameActionResult result)
        => new(true, result, string.Empty);

    /// <summary>
    /// Creates a failed execute action result.
    /// </summary>
    public static ExecuteActionResult Failed(string errorMessage)
        => new(false, default(GameActionResult), errorMessage);
}

/// <summary>
/// Represents the result of ending a casino game session.
/// </summary>
public sealed record EndGameResult(
    bool Success,
    GameActionResult FinalResult,
    string ErrorMessage)
{
    /// <summary>
    /// Creates a successful end game result.
    /// </summary>
    public static EndGameResult Successful(GameActionResult finalResult)
        => new(true, finalResult, string.Empty);

    /// <summary>
    /// Creates a failed end game result.
    /// </summary>
    public static EndGameResult Failed(string errorMessage)
        => new(false, new GameActionResult(true, false, 0, errorMessage), errorMessage);

    /// <summary>
    /// Creates a failed end game result with a custom final result.
    /// </summary>
    public static EndGameResult Failed(GameActionResult finalResult, string errorMessage)
        => new(false, finalResult, errorMessage);
}
