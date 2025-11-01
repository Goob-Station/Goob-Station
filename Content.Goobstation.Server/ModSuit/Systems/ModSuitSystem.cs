using Content.Goobstation.Shared.ModSuits;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Inventory;

namespace Content.Goobstation.Server.ModSuits;

public sealed partial class ModSuitSystem : SharedModSuitSystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ModSuitComponent, InventoryRelayedEvent<FindBatteryEvent>>(OnFindInventoryBatteryEvent);
    }

    /// <summary>
    /// Tries to find battery for charger
    /// </summary>
    private void OnFindInventoryBatteryEvent(Entity<ModSuitComponent> entity, ref InventoryRelayedEvent<FindBatteryEvent> args)
    {
        if (args.Args.FoundBattery.HasValue)
            return;

        if (_itemSlotsSystem.TryGetSlot(entity.Owner, "cell_slot", out ItemSlot? slot) && TryComp<BatteryComponent>(slot.Item, out var battery))
            args.Args.FoundBattery = (slot.Item.Value, battery);
    }
}
