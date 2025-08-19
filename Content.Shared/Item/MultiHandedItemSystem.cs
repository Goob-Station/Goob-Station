// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared.Item;

public sealed class MultiHandedItemSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!; // Goobstation

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<MultiHandedItemComponent, GettingPickedUpAttemptEvent>(OnAttemptPickup);
        SubscribeLocalEvent<MultiHandedItemComponent, VirtualItemDeletedEvent>(OnVirtualItemDeleted);
        SubscribeLocalEvent<MultiHandedItemComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<MultiHandedItemComponent, GotUnequippedHandEvent>(OnUnequipped);

        // Goobstation
        SubscribeLocalEvent<MultiHandedItemComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<MultiHandedItemComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnEquipped(Entity<MultiHandedItemComponent> ent, ref GotEquippedHandEvent args)
    {
        for (var i = 0; i < ent.Comp.HandsNeeded - 1; i++)
        {
            _virtualItem.TrySpawnVirtualItemInHand(ent.Owner, args.User);
        }
    }

    private void OnUnequipped(Entity<MultiHandedItemComponent> ent, ref GotUnequippedHandEvent args)
    {
        _virtualItem.DeleteInHandsMatching(args.User, ent.Owner);
    }

    private void OnAttemptPickup(Entity<MultiHandedItemComponent> ent, ref GettingPickedUpAttemptEvent args)
    {
        if (TryComp<HandsComponent>(args.User, out var hands) && hands.CountFreeHands() >= ent.Comp.HandsNeeded)
            return;

        args.Cancel();
        _popup.PopupPredictedCursor(Loc.GetString("multi-handed-item-pick-up-fail",
            ("number", ent.Comp.HandsNeeded - 1), ("item", ent.Owner)), args.User);
    }

    private void OnVirtualItemDeleted(Entity<MultiHandedItemComponent> ent, ref VirtualItemDeletedEvent args)
    {
        if (args.BlockingEntity != ent.Owner || _timing.ApplyingState)
            return;

        _hands.TryDrop(args.User, ent.Owner);
    }

    // everything below is Goobstation
    private void OnComponentStartup(Entity<MultiHandedItemComponent> ent, ref ComponentStartup args)
    {
        if (_timing.ApplyingState
            || !_container.TryGetContainingContainer((ent, null, null), out var container)
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
            if (_timing.InPrediction
                || !TryComp(hand.HeldEntity, out VirtualItemComponent? virt)
                || virt.BlockingEntity != ent.Owner)
                continue;

            if (TerminatingOrDeleted(hand.HeldEntity))
                return;

            QueueDel(hand.HeldEntity);
        }
    }
}
