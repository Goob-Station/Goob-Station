using Content.Shared.Containers.ItemSlots;
using Content.Server.Store.Systems;
using Content.Shared.Store.Components;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.NTR;

public sealed class CorporateOverrideSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CorporateOverrideComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CorporateOverrideComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<CorporateOverrideComponent, EntRemovedFromContainerMessage>(OnItemRemoved);
    }

    private void OnInit(EntityUid uid, CorporateOverrideComponent comp, ComponentInit args)
    {
        comp.OverrideSlot = _container.EnsureContainer<ContainerSlot>(uid, "CorporateOverrideSlot");
    } // shitcod was taken from the sharedvehiclesystem

    private void OnItemInserted(EntityUid uid, CorporateOverrideComponent comp, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != "CorporateOverrideSlot" || !TryComp<StoreComponent>(uid, out var store))
            return;

        if (!store.Categories.Contains(comp.UnlockedCategory))
        {
            store.Categories.Add(comp.UnlockedCategory);
            Dirty(uid, store);
        }
    }

    private void OnItemRemoved(EntityUid uid, CorporateOverrideComponent comp, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != "CorporateOverrideSlot" || !TryComp<StoreComponent>(uid, out var store))
            return;

        if (store.Categories.Contains(comp.UnlockedCategory))
        {
            store.Categories.Remove(comp.UnlockedCategory);
            Dirty(uid, store);
        }
    }
}
