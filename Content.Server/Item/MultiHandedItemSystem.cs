// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Inventory;
using Content.Shared.Hands;
using Content.Shared.Item;

// Goobstation usings
using System.Linq;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory.VirtualItem;
using Robust.Shared.Containers;

namespace Content.Server.Item;

public sealed class MultiHandedItemSystem : SharedMultiHandedItemSystem
{
    [Dependency] private readonly VirtualItemSystem _virtualItem = default!;

    // Goobstation dependencies
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    // Goobstation
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MultiHandedItemComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<MultiHandedItemComponent, ComponentShutdown>(OnComponentShutdown);
    }

    protected override void OnEquipped(EntityUid uid, MultiHandedItemComponent component, GotEquippedHandEvent args)
    {
        for (var i = 0; i < component.HandsNeeded - 1; i++)
        {
            _virtualItem.TrySpawnVirtualItemInHand(uid, args.User);
        }
    }

    protected override void OnUnequipped(EntityUid uid, MultiHandedItemComponent component, GotUnequippedHandEvent args)
    {
        _virtualItem.DeleteInHandsMatching(args.User, uid);
    }

    // everything below is Goobstation
    private void OnComponentStartup(Entity<MultiHandedItemComponent> ent, ref ComponentStartup args)
    {
        if (!_container.TryGetContainingContainer((ent, null, null), out var container)
            || !HasComp<HandsComponent>(container.Owner))
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
            _hands.TryDrop(container.Owner, droppable[i]);

        for (var i = 1; i < ent.Comp.HandsNeeded; i++)
            _virtualItem.TrySpawnVirtualItemInHand(ent, container.Owner);
    }

    private void OnComponentShutdown(Entity<MultiHandedItemComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        // Method exists for that but it calls an event on deleting the virtual item hence forces the item to drop
        foreach (var hand in _hands.EnumerateHands(Transform(ent).ParentUid))
        {
            if (!TryComp(hand.HeldEntity, out VirtualItemComponent? virt)
                || virt.BlockingEntity != ent.Owner)
                continue;

            if (TerminatingOrDeleted(hand.HeldEntity))
                return;

            QueueDel(hand.HeldEntity);
        }
    }
}
