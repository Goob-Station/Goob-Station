using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Materials;

namespace Content.Shared.Containers.ItemSlots;

// Goobstation & Lavaland partial for the main ItemSlots system
public sealed partial class ItemSlotsSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    private void InitializeGoob()
    {
        SubscribeLocalEvent<ItemSlotsComponent, GotReclaimedEvent>(OnReclaimed);
        SubscribeLocalEvent<ItemSlotsComponent, ItemSlotInteractionDoAfterEvent>(HandleDoAfter);
    }

    public bool TryInsertWithConditions(EntityUid uid, ItemSlotsComponent itemSlots, EntityUid user, EntityUid toInsert, bool doAfter = true)
    {
        if (!TryComp(user, out HandsComponent? hands))
            return false;

        if (itemSlots.Slots.Count == 0)
            return false;

        // If any slot can be inserted into don't show popup.
        // If any whitelist passes, but slot is locked, then show locked.
        // If whitelist fails all, show whitelist fail.

        // valid, insertable slots (if any)
        var slots = new List<ItemSlot>();

        string? whitelistFailPopup = null;
        string? lockedFailPopup = null;
        foreach (var slot in itemSlots.Slots.Values)
        {
            if (!slot.InsertOnInteract)
                continue;

            if (CanInsert(uid, toInsert, user, slot, slot.Swap))
            {
                slots.Add(slot);
                break; //Goobstation: If an item has multiple ItemSlots, stick with the highest priority and stop looking.
            }
            else
            {
                var allowed = CanInsertWhitelist(toInsert, slot);
                if (lockedFailPopup == null && slot.LockedFailPopup != null && allowed && slot.Locked)
                    lockedFailPopup = slot.LockedFailPopup;

                if (whitelistFailPopup == null && slot.WhitelistFailPopup != null)
                    whitelistFailPopup = slot.WhitelistFailPopup;
            }
        }

        if (slots.Count == 0)
        {
            // it's a bit weird that the popupMessage is stored with the item slots themselves, but in practice
            // the popup messages will just all be the same, so it's probably fine.
            //
            // doing a check to make sure that they're all the same or something is probably frivolous
            if (lockedFailPopup != null)
                _popupSystem.PopupClient(Loc.GetString(lockedFailPopup), uid, user);
            else if (whitelistFailPopup != null)
                _popupSystem.PopupClient(Loc.GetString(whitelistFailPopup), uid, user);
            return false;
        }
        slots.Sort(SortEmpty);

        foreach (var slot in slots)
        {
            TryInsertOrDoAfter(uid, (user, hands), toInsert, slot, doAfter);
        }

        return false;
    }

    /// <summary>
    /// Tries to start a do-after if it can, otherwise
    /// </summary>
    public bool TryInsertOrDoAfter(EntityUid uid, Entity<HandsComponent?> user, EntityUid toInsert, ItemSlot slot, bool doAfter = true)
    {
        // Handle do-after insert
        if (doAfter && TryStartInsertDoAfter(slot, toInsert, user))
            return true; // We are delaying it to some time

        // Drop the held item onto the floor. Return if the user cannot drop.
        if (_handsSystem.IsHolding(user, toInsert) && !_handsSystem.TryDrop(user, toInsert)) // Goobstation - don't try to drop if not holding
            return false;

        if (slot.Item != null)
            _handsSystem.TryPickupAnyHand(user, slot.Item.Value, handsComp: user.Comp);

        Insert(uid, slot, toInsert, user, excludeUserAudio: true);

        if (slot.InsertSuccessPopup.HasValue)
            _popupSystem.PopupClient(Loc.GetString(slot.InsertSuccessPopup), uid, user);
        return true;
    }

    private bool TryStartInsertDoAfter(ItemSlot slot, EntityUid item, EntityUid? user)
    {
        if (slot.InsertDelay != null && user != null)
        {
            return _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
                user.Value,
                slot.InsertDelay.Value,
                new ItemSlotInteractionDoAfterEvent(slot.ID!, false, true),
                slot.ContainerSlot?.Owner,
                item,
                item)
            {
                BreakOnHandChange = true,
                BreakOnMove = true,
                BreakOnDropItem = true,
                BreakOnDamage = true,
            });
        }

        return false;
    }

    private bool TryStartEjectDoAfter(ItemSlot slot, EntityUid item, EntityUid? user)
    {
        if (slot.EjectDelay != null && user != null)
        {
            return _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
                user.Value,
                slot.EjectDelay.Value,
                new ItemSlotInteractionDoAfterEvent(slot.ID!, true, false),
                item)
            {
                BreakOnHandChange = true,
                BreakOnMove = true,
                BreakOnDropItem = true,
                BreakOnDamage = true,
            });
        }

        return false;
    }

    private void OnReclaimed(EntityUid uid, ItemSlotsComponent component, GotReclaimedEvent args)
    {
        foreach (var slot in component.Slots.Values)
        {
            if (slot.ContainerSlot != null)
                _containers.EmptyContainer(slot.ContainerSlot, destination: args.ReclaimerCoordinates);
        }
    }

    private void HandleDoAfter(EntityUid uid, ItemSlotsComponent component, ItemSlotInteractionDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled
            || !component.Slots.TryGetValue(args.SlotId, out var slot))
            return;

        if (args.TryEject && slot.HasItem)
            TryEjectToHands(uid, slot, args.User, true, false);
        else if (args.TryInsert && !slot.HasItem && args.Used != null)
            TryInsertWithConditions(uid, component, args.User, args.Used.Value, false);
    }
}
