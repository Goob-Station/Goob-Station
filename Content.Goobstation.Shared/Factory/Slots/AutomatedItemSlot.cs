using Content.Shared.Containers.ItemSlots;

namespace Content.Goobstation.Shared.Factory.Slots;

/// <summary>
/// Abstraction over an <see cref="ItemSlot"/> on the machine.
/// </summary>
public sealed partial class AutomatedItemSlot : AutomationSlot
{
    [Dependency] private readonly ItemSlotsSystem _slots = default!;

    /// <summary>
    /// The name of the slot to automate.
    /// </summary>
    [DataField(required: true)]
    public string Slot = string.Empty;

    public override bool Insert(EntityUid uid, EntityUid item, EntityUid user)
    {
        // don't pass user since no reason to admin log a robotic arm
        return _slots.TryInsert(uid, Slot, item, user: null);
    }

    public override EntityUid? GetItem(EntityUid uid, EntityUid user)
    {
        return _slots.GetItemOrNull(uid, Slot);
    }
}
