using Robust.Shared.Network;

namespace Content.Shared._Goobstation.ServerCurrency.Events;

/// <summary>
///     Arguments for when a player's currency is changed.
/// </summary>
[ByRefEvent]
public readonly record struct PlayerBalanceChangeEvent
{
    /// <summary>
    ///     Creates a new instance of this class.
    /// </summary>
    public PlayerBalanceChangeEvent(EntityUid entId, NetUserId userId,  int newAmount, int oldAmount)
    {
        EntID = entId;
        UserID = userId;
        NewAmount = newAmount;
        OldAmount = oldAmount;
    }

    /// <summary>
    ///     NetUserId of the player with the currency change.
    /// </summary>
    public readonly EntityUid EntID;

    /// <summary>
    ///     NetUserId of the player with the currency change.
    /// </summary>
    public readonly NetUserId UserID;

    /// <summary>
    ///     New amount that replaced the old one.
    /// </summary>
    public readonly int NewAmount;

    /// <summary>
    ///     Old amount that was replaced.
    /// </summary>
    public readonly int OldAmount;
}
