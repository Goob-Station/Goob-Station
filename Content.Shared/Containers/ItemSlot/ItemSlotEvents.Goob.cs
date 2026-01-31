using Content.Shared.DoAfter; // Goobstation
using Robust.Shared.Serialization;

namespace Content.Shared.Containers.ItemSlots;

/// <summary>
/// Goobstation
/// A do-after event for inserting-removing from slots with a delay.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class ItemSlotInteractionDoAfterEvent : DoAfterEvent
{
    /// <summary>
    ///     The name of the slot/container from which to insert or eject an item.
    /// </summary>
    public string SlotId;

    /// <summary>
    ///     Whether to attempt to insert an item into the slot, if there is not already one inside.
    /// </summary>
    public bool TryInsert;

    /// <summary>
    ///     Whether to attempt to eject the item from the slot, if it has one.
    /// </summary>
    public bool TryEject;

    public ItemSlotInteractionDoAfterEvent(string slotId, bool tryEject = true, bool tryInsert = true)
    {
        SlotId = slotId;
        TryEject = tryEject;
        TryInsert = tryInsert;
    }

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
