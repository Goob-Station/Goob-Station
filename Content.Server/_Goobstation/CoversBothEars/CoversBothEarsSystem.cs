using Content.Shared._Goobstation.CoversBothEars;
using Content.Shared.Clothing;
using Content.Shared.Inventory.VirtualItem;

namespace Content.Server._Goobstation.CoversBothEars;

/// <summary>
/// This handles spawning a virtual item on the opposite ear for items like headsets.
/// </summary>
public sealed class CoversBothEarsSystem : EntitySystem
{
    [Dependency] private readonly SharedVirtualItemSystem _virtualItemSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<CoversBothEarsComponent, ClothingGotEquippedEvent>(OnGotEquippedEvent);
        SubscribeLocalEvent<CoversBothEarsComponent, ClothingGotUnequippedEvent>(OnGotUnequippedEvent);
    }

    private void OnGotUnequippedEvent(Entity<CoversBothEarsComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        if(ent.Comp.VirtualEnt == EntityUid.Invalid)
            return;

        if(!TryComp<VirtualItemComponent>(ent.Comp.VirtualEnt, out var virtualItemComponent))
            return;

        _virtualItemSystem.DeleteVirtualItem((ent.Comp.VirtualEnt, virtualItemComponent), args.Wearer);
        ent.Comp.VirtualEnt = EntityUid.Invalid;
    }

    private void OnGotEquippedEvent(Entity<CoversBothEarsComponent> ent, ref ClothingGotEquippedEvent args)
    {
        if(ent.Comp.VirtualEnt != EntityUid.Invalid)
            return;

        if (args.Clothing.InSlot == CoversBothEarsComponent.EarsSlot)
            if(_virtualItemSystem.TrySpawnVirtualItemInInventory(ent, args.Wearer, CoversBothEarsComponent.Ears2Slot, true, out var virtualEntity))
                ent.Comp.VirtualEnt = virtualEntity.Value;

        if (args.Clothing.InSlot == CoversBothEarsComponent.Ears2Slot)
            if(_virtualItemSystem.TrySpawnVirtualItemInInventory(ent, args.Wearer, CoversBothEarsComponent.EarsSlot, true, out var virtualEntity))
                ent.Comp.VirtualEnt = virtualEntity.Value;
    }
}
