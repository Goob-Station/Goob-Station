using Content.Shared.Armor;

namespace Content.Shared.Inventory;

public abstract partial class InventorySystem
{
    /// <summary>
    /// Whether a <c>DependsOnAny</c> provider passes the slot's component and suit storage whitelist checks
    /// </summary>
    private bool DependencyAllowsItem(EntityUid provider, SlotDefinition slotDefinition, EntityUid itemUid)
    {
        if (slotDefinition.DependsOnComponents is not {} componentRegistry)
            return true;

        foreach (var (_, entry) in componentRegistry)
        {
            if (!HasComp(provider, entry.Component.GetType()))
                return false;

            if (TryComp<AllowSuitStorageComponent>(provider, out var comp) && _whitelistSystem.IsWhitelistFailOrNull(comp.Whitelist, itemUid))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Whether the item in a <c>DependsOnAny</c> slot is still allowed by at least one of its provider slots
    /// </summary>
    private bool AnyDependencyAllowsSlotItem(EntityUid target, SlotDefinition slotDef, InventoryComponent? inventory)
    {
        // Nothing in the slot, so nothing to drop
        if (!TryGetSlotEntity(target, slotDef.Name, out var item, inventory))
            return true;

        foreach (var providerSlot in slotDef.DependsOnAny!)
        {
            if (TryGetSlotEntity(target, providerSlot, out var provider, inventory) && DependencyAllowsItem(provider.Value, slotDef, item.Value))
                return true;
        }

        return false;
    }
}
