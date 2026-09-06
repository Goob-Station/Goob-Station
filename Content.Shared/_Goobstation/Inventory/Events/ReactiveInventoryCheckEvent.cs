using Content.Shared.Chemistry.Components;
using Content.Shared.Inventory;

namespace Content.Shared._Goobstation.Inventory.Events;

/// <summary>
/// Raised when entity is hit by entity with SpillableComponent
/// </summary>
[ByRefEvent]
public record struct ReactiveInventoryCheckEvent(Solution SplitSol, EntityUid Victim) : IInventoryRelayEvent
{
    public readonly SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;

    /// <summary>
    /// If the event is cancelled
    /// </summary>
    public bool Cancelled;
}
