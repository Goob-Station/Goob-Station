using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Shared.AbilitySuppression;

[RegisterComponent, NetworkedComponent]
public sealed partial class MagicSuppressionComponent : Component
{
}

/// <summary>
///     Raised when you want to check if an item that suppresses the users magic is worn
/// </summary>
public sealed class CheckMagicSuppressionEvent : IInventoryRelayEvent
{
    public bool Cancelled;

    /// <summary>
    ///     Define what is blocking the spellcast
    /// </summary>
    public EntityUid Blocker;

    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}
