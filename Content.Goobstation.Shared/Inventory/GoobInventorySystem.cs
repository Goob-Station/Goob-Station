// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.Polymorph;
using Content.Shared.Body.Organ;
using Content.Shared.Inventory;
using Content.Shared.Mind.Components;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Inventory;

public sealed partial class GoobInventorySystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<MetaDataComponent> _metaQuery;
    private EntityQuery<ContainerManagerComponent> _managerQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();
        InitializeRelays();

        _metaQuery = GetEntityQuery<MetaDataComponent>();
        _managerQuery = GetEntityQuery<ContainerManagerComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();

        SubscribeLocalEvent<InventoryComponent, BeforePolymorphedEvent>(OnBeforePolymorphed);
    }

    private void OnBeforePolymorphed(Entity<InventoryComponent> ent, ref BeforePolymorphedEvent args)
    {
        HashSet<EntityUid> toRemove = new();

        foreach (var uid in _inventorySystem.GetHandOrInventoryEntities((ent.Owner, null, ent.Comp)))
        {
            if (!_managerQuery.TryGetComponent(uid, out var containerManager))
                continue;

            foreach (var container in _container.GetAllContainers(uid, containerManager))
            {
                if (!IsContainerValid(container.Owner, toRemove))
                    continue;

                foreach (var item in container.ContainedEntities)
                {
                    if (HasComp<MindContainerComponent>(item) && !HasComp<OrganComponent>(item))
                        toRemove.Add(item);
                }
            }
        }

        foreach (var remove in toRemove)
        {
            _transform.AttachToGridOrMap(remove);
        }
    }

    private bool IsContainerValid(EntityUid uid, HashSet<EntityUid> toRemove)
    {
        if (toRemove.Contains(uid))
            return false;

        var parent = _xformQuery.Comp(uid).ParentUid;
        var child = uid;

        while (parent.IsValid())
        {
            if ((_metaQuery.GetComponent(child).Flags & MetaDataFlags.InContainer) == MetaDataFlags.InContainer &&
                _managerQuery.TryGetComponent(parent, out var conManager) &&
                _container.TryGetContainingContainer(parent, child, out var parentContainer, conManager) &&
                toRemove.Contains(parentContainer.Owner))
                return false;

            var parentXform = _xformQuery.GetComponent(parent);
            child = parent;
            parent = parentXform.ParentUid;
        }

        return true;
    }
}
