using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Shared._RMC14.Clothing;

public sealed class RMCClothingSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    private EntityQuery<ClothingLimitComponent> _clothingLimitQuery;

    public override void Initialize()
    {
        _clothingLimitQuery = GetEntityQuery<ClothingLimitComponent>();

        SubscribeLocalEvent<ClothingLimitComponent, BeingEquippedAttemptEvent>(OnClothingLimitBeingEquippedAttempt);
    }

    private void OnClothingLimitBeingEquippedAttempt(Entity<ClothingLimitComponent> ent, ref BeingEquippedAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if ((args.SlotFlags & ent.Comp.Slot) == 0)
            return;

        var slots = _inventory.GetSlotEnumerator(args.EquipTarget, ent.Comp.Slot);
        while (slots.MoveNext(out var slot))
        {
            if (_clothingLimitQuery.TryComp(slot.ContainedEntity, out var otherLimit) &&
                otherLimit.Id == ent.Comp.Id)
            {
                args.Reason = "rmc-clothing-limit";
                args.Cancel();
            }
        }
    }
}
