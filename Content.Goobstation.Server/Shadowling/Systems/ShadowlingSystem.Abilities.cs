using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Server.Storage.Components;
using Content.Shared.Inventory;

namespace Content.Goobstation.Server.Shadowling.Systems;

/// <summary>
/// This handles only the Hatching Ability
/// </summary>
public sealed partial class ShadowlingSystem
{
    public void SubscribeAbilities()
    {
        SubscribeLocalEvent<ShadowlingComponent, HatchEvent>(OnHatch);
    }

    private void OnHatch(EntityUid uid, ShadowlingComponent comp, HatchEvent args)
    {
        args.Handled = true;
        _actions.RemoveAction(uid, (args.Action.Owner, args.Action.Comp));
        StartHatchingProgress(uid, comp);
    }

    private void StartHatchingProgress(EntityUid uid, ShadowlingComponent comp)
    {
        comp.IsHatching = true;

        // Drop all items
        if (TryComp<InventoryComponent>(uid, out var inv))
        {
            foreach (var slot in inv.Slots)
                _inventorySystem.DropSlotContents(uid, slot.Name, inv);
        }

        var egg = SpawnAtPosition(comp.Egg, Transform(uid).Coordinates);
        if (TryComp<HatchingEggComponent>(egg, out var eggComp)
            && TryComp<EntityStorageComponent>(egg, out var eggStorage))
        {
            eggComp.ShadowlingInside = uid;
            _entityStorage.Insert(uid, egg, eggStorage);
        }

        // It should be noted that Shadowling shouldn't be able to take damage during this process.
    }
}
