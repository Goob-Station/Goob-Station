// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Store.Components;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.NTR;

public sealed class CorporateOverrideSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CorporateOverrideComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CorporateOverrideComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
        SubscribeLocalEvent<CorporateOverrideComponent, EntRemovedFromContainerMessage>(OnItemRemoved);
    }

    private void OnInit(EntityUid uid, CorporateOverrideComponent comp, ComponentInit args) =>
        comp.OverrideSlot = _container.EnsureContainer<ContainerSlot>(uid, CorporateOverrideComponent.ContainerId);

    private void OnItemInserted(EntityUid uid, CorporateOverrideComponent comp, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != CorporateOverrideComponent.ContainerId
            || !TryComp<StoreComponent>(uid, out var store))
            return;

        if (store.Categories.Add(comp.UnlockedCategory))
            Dirty(uid, store);
    }

    private void OnItemRemoved(EntityUid uid, CorporateOverrideComponent comp, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != CorporateOverrideComponent.ContainerId
            || !TryComp<StoreComponent>(uid, out var store))
            return;

        if (!store.Categories.Contains(comp.UnlockedCategory))
            return;
            
        store.Categories.Remove(comp.UnlockedCategory);
        Dirty(uid, store);
    }
}
