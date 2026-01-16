namespace Content.Goobstation.Shared.Players;

/// <summary>
/// Shared interface for tracking whether the player joined as an observer.
/// </summary>
public interface IObserverStatusManager
{
    /// <summary>
    /// Whether the player joined as an observer through the lobby.
    /// </summary>
    bool JoinedAsObserver { get; }

    /// <summary>
    /// Whether the player is an admin (including deadminned admins).
    /// Used to bypass observer restrictions.
    /// </summary>
    bool IsAdmin { get; }
}
