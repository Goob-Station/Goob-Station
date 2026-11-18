using Content.Client.Clothing;
using Content.Client.Toggleable;
using Content.Goobstation.Shared.ModSuits.Components;
using Content.Shared.Clothing;
using Content.Shared.Inventory;

namespace Content.Goobstation.Client.ModSuits;

public sealed partial class ModHiddenSlotVisualsSystem : EntitySystem
{
    [Dependency] private InventorySystem _inventory = default!;

    [SubscribeLocalEvent(after: [typeof(ClientClothingSystem), typeof(ModSuitVisualizerSystem), typeof(ToggleableVisualsSystem), typeof(ModHatVisualsSystem)])]
    private void OnGetEquipmentVisuals(Entity<ModPartComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (_inventory.TryGetSlot(args.Equipee, args.Slot, out var slot) && !slot.ShowInWindow)
            args.Layers.Clear();
    }
}
