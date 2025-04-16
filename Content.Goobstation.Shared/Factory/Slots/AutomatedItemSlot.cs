using Content.Goobstation.Shared.Factory.Filters;
using Content.Shared.Containers.ItemSlots;

namespace Content.Goobstation.Shared.Factory.Slots;

/// <summary>
/// Abstraction over an <see cref="ItemSlot"/> on the machine.
/// </summary>
public sealed partial class AutomatedItemSlot : AutomationSlot
{
    /// <summary>
    /// The name of the slot to automate.
    /// </summary>
    [DataField(required: true)]
    public string SlotId = string.Empty;

    private ItemSlotsSystem? _slots;

    // Dependency doesnt work for whatever reason
    public ItemSlotsSystem Slots
    {
        get
        {
            _slots ??= EntMan.System<ItemSlotsSystem>();
            return _slots;
        }
    }

    private ItemSlot? _slot;

    [ViewVariables]
    public ItemSlot? GetSlot(EntityUid uid)
    {
        if (_slot == null)
            Slots.TryGetSlot(uid, SlotId, out _slot);
        return _slot;
    }

    public override bool Insert(EntityUid uid, EntityUid item)
    {
        if (!base.Insert(uid, item))
            return false;

        if (GetSlot(uid) is not {} slot)
            return false;

        return Slots.TryInsert(uid, slot, item, user: null);
    }

    public override bool CanInsert(EntityUid uid, EntityUid item)
    {
        if (!base.CanInsert(uid, item))
            return false;

        if (GetSlot(uid) is not {} slot)
            return false;

        return Slots.CanInsert(uid, usedUid: item, user: null, slot);
    }

    public override EntityUid? GetItem(EntityUid uid, AutomationFilter? filter)
    {
        if (GetSlot(uid)?.Item is not {} item || (filter?.IsDenied(item) ?? false))
            return null;

        return item;
    }
}
