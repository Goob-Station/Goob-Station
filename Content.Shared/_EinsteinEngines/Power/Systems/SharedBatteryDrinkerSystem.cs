using System.Diagnostics.CodeAnalysis;
using Content.Shared._EinsteinEngines.Silicon.Charge;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.PowerCell.Components;
using Content.Shared.Weapons.Ranged;

namespace Content.Shared._EinsteinEngines.Power.Systems;

// Goobstation - Energycrit: BatteryDrinkerSystem verb prediction
public abstract class SharedBatteryDrinkerSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    /// <summary>
    ///     Find an item that a battery drinker can drink from without using BatterySystem
    /// </summary>
    public bool SearchForSource(EntityUid ent, [NotNullWhen(true)] out EntityUid? source)
    {
        // Are we a battery drinker source
        if (HasComp<BatteryDrinkerSourceComponent>(ent))
        {
            source = ent;
            return true;
        }

        // Do we contain a source?
        if (SearchForCellSlot(ent, out var slot) &&
            slot.HasItem && HasComp<BatteryDrinkerSourceComponent>(slot.Item))
        {
            source = slot.Item;
            return true;
        }

        // We found nothing
        source = null;
        return false;
    }

    /// <summary>
    ///     Find a battery that can be charged without using BatterySystem
    /// </summary>
    public bool SearchForDrinker(EntityUid ent, [NotNullWhen(true)] out EntityUid? drinker)
    {
        drinker = null;

        // Do we have a cell slot
        if (SearchForCellSlot(ent, out var slot)) {
            // Do we have a battery to charge?
            if (slot.HasItem && HasComp<BatteryDrinkerSourceComponent>(slot.Item))
            {
                drinker = slot.Item;
                return true;
            }

            // We don't have a battery to charge
            return false;
        }

        // We don't have a power cell slot, assume it's inside us
        drinker = ent;
        return true;
    }

    /// <summary>
    ///     Find a battery cell slot that we can be drink from without ChargerSystem
    /// </summary>
    private bool SearchForCellSlot(EntityUid ent, [NotNullWhen(true)] out ItemSlot? slot)
    {
        slot = null;

        // Check if we have any item slots
        if (!HasComp<ItemSlotsComponent>(ent))
            return false;

        // Check for power cell slot
        if (TryComp<PowerCellSlotComponent>(ent, out var powerCellSlot)
            && _itemSlots.TryGetSlot(ent, powerCellSlot.CellSlotId, out slot))
            return true;

        // Check for laser magazine
        if (HasComp<MagazineAmmoProviderComponent>(ent)
            && _itemSlots.TryGetSlot(ent, "gun_magazine", out slot))
            return true;

        // No slot was found
        return false;
    }
}
