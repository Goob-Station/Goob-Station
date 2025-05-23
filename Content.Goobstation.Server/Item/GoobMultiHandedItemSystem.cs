// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Item;

public sealed class GoobMultiHandedItemSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MultiHandedItemComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<MultiHandedItemComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentInit(Entity<MultiHandedItemComponent> ent, ref ComponentInit args)
    {
        if (!_container.TryGetContainingContainer((ent, null, null), out var container))
            return;

        // dropOthers: true in TrySpawnVirtualItemInHand didn't work properly so here we have this linq monstrosity
        var hands = _hands.EnumerateHands(container.Owner).Where(hand => hand.HeldEntity != ent).ToList();
        var iterations = ent.Comp.HandsNeeded - 1 - hands.Count(hand => hand.IsEmpty);
        var droppable = hands.Where(hand => _hands.CanDropHeld(container.Owner, hand, false)).ToList();

        if (iterations > droppable.Count)
        {
            _hands.TryDrop(container.Owner, ent);
            return;
        }

        for (var i = 0; i < iterations; i++)
        {
            _hands.TryDrop(container.Owner, droppable[i]);
        }

        for (var i = 1; i < ent.Comp.HandsNeeded; i++)
        {
            _virtualItem.TrySpawnVirtualItemInHand(ent, container.Owner);
        }
    }

    private void OnComponentShutdown(Entity<MultiHandedItemComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        // Method exists for that but it calls an event on deleting the virtual item hence forces the item to drop
        foreach (var hand in _hands.EnumerateHands(Transform(ent).ParentUid))
        {
            if (!TryComp(hand.HeldEntity, out VirtualItemComponent? virt) || virt.BlockingEntity != ent.Owner)
                continue;

            if (TerminatingOrDeleted(hand.HeldEntity))
                return;

            QueueDel(hand.HeldEntity);
        }
    }
}
