// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <jmaster9999@gmail.com>
// SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Menshin <Menshin@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 PixelTK <85175107+PixelTheKermit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JustCone <141039037+JustCone14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 coolboy911 <85909253+coolboy911@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 saintmuntzer <47153094+saintmuntzer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Network;

namespace Content.Shared.Hands.EntitySystems;

public abstract partial class SharedHandsSystem
{
    [Dependency] private readonly INetManager _net = default!; // Goobstation
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] protected readonly SharedContainerSystem ContainerSystem = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualSystem = default!;

    protected event Action<Entity<HandsComponent>?>? OnHandSetActive;

    public override void Initialize()
    {
        base.Initialize();

        InitializeInteractions();
        InitializeDrop();
        InitializePickup();
        InitializeRelay();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        CommandBinds.Unregister<SharedHandsSystem>();
    }

    public virtual void AddHand(EntityUid uid, string handName, HandLocation handLocation, HandsComponent? handsComp = null)
    {
        if (!Resolve(uid, ref handsComp, false))
            return;

        if (handsComp.Hands.ContainsKey(handName))
            return;

        var container = ContainerSystem.EnsureContainer<ContainerSlot>(uid, handName);
        container.OccludesLight = false;

        var newHand = new Hand(handName, handLocation, container);
        handsComp.Hands.Add(handName, newHand);
        AddToSortedHands(handsComp, handName, handLocation); // Shitmed Change

        if (handsComp.ActiveHand == null)
            SetActiveHand(uid, newHand, handsComp);

        RaiseLocalEvent(uid, new HandCountChangedEvent(uid));
        Dirty(uid, handsComp);
    }

    public virtual void RemoveHand(EntityUid uid, string handName, HandsComponent? handsComp = null)
    {
        if (!Resolve(uid, ref handsComp, false))
            return;

        if (!handsComp.Hands.Remove(handName, out var hand))
            return;

        handsComp.SortedHands.Remove(hand.Name);
        TryDrop(uid, hand, null, false, true, handsComp);
        if (hand.Container != null)
            ContainerSystem.ShutdownContainer(hand.Container);

        if (handsComp.ActiveHand == hand)
            TrySetActiveHand(uid, handsComp.SortedHands.FirstOrDefault(), handsComp);

        RaiseLocalEvent(uid, new HandCountChangedEvent(uid));
        Dirty(uid, handsComp);
    }

    /// <summary>
    /// Gets rid of all the entity's hands.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="handsComp"></param>
    public void RemoveHands(EntityUid uid, HandsComponent? handsComp = null)
    {
        if (!Resolve(uid, ref handsComp))
            return;

        RemoveHands(uid, EnumerateHands(uid), handsComp);
    }

    private void RemoveHands(EntityUid uid, IEnumerable<Hand> hands, HandsComponent handsComp)
    {
        if (!hands.Any())
            return;

        var hand = hands.First();
        RemoveHand(uid, hand.Name, handsComp);

        // Repeats it for any additional hands.
        RemoveHands(uid, hands, handsComp);
    }

    private void HandleSetHand(RequestSetHandEvent msg, EntitySessionEventArgs eventArgs)
    {
        if (eventArgs.SenderSession.AttachedEntity == null)
            return;

        TrySetActiveHand(eventArgs.SenderSession.AttachedEntity.Value, msg.HandName);
    }

    /// <summary>
    ///     Get any empty hand. Prioritizes the currently active hand.
    /// </summary>
    public bool TryGetEmptyHand(EntityUid uid, [NotNullWhen(true)] out Hand? emptyHand, HandsComponent? handComp = null)
    {
        emptyHand = null;
        if (!Resolve(uid, ref handComp, false))
            return false;

        foreach (var hand in EnumerateHands(uid, handComp))
        {
            if (hand.IsEmpty)
            {
                emptyHand = hand;
                return true;
            }
        }

        return false;
    }

    public bool TryGetActiveHand(Entity<HandsComponent?> entity, [NotNullWhen(true)] out Hand? hand)
    {
        if (!Resolve(entity, ref entity.Comp, false))
        {
            hand = null;
            return false;
        }

        hand = entity.Comp.ActiveHand;
        return hand != null;
    }

    public bool TryGetActiveItem(Entity<HandsComponent?> entity, [NotNullWhen(true)] out EntityUid? item)
    {
        if (!TryGetActiveHand(entity, out var hand))
        {
            item = null;
            return false;
        }

        item = hand.HeldEntity;
        return item != null;
    }

    /// <summary>
    /// Gets active hand item if relevant otherwise gets the entity itself.
    /// </summary>
    public EntityUid GetActiveItemOrSelf(Entity<HandsComponent?> entity)
    {
        if (!TryGetActiveItem(entity, out var item))
        {
            return entity.Owner;
        }

        return item.Value;
    }

    public Hand? GetActiveHand(Entity<HandsComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
            return null;

        return entity.Comp.ActiveHand;
    }

    public EntityUid? GetActiveItem(Entity<HandsComponent?> entity)
    {
        return GetActiveHand(entity)?.HeldEntity;
    }

    /// <summary>
    ///     Enumerate over hands, starting with the currently active hand.
    /// </summary>
    public IEnumerable<Hand> EnumerateHands(EntityUid uid, HandsComponent? handsComp = null)
    {
        if (!Resolve(uid, ref handsComp, false))
            yield break;

        if (handsComp.ActiveHand != null)
            yield return handsComp.ActiveHand;

        foreach (var name in handsComp.SortedHands)
        {
            if (name != handsComp.ActiveHand?.Name)
                yield return handsComp.Hands[name];
        }
    }

    /// <summary>
    ///     Enumerate over held items, starting with the item in the currently active hand (if there is one).
    /// </summary>
    public IEnumerable<EntityUid> EnumerateHeld(EntityUid uid, HandsComponent? handsComp = null)
    {
        if (!Resolve(uid, ref handsComp, false))
            yield break;

        if (handsComp.ActiveHandEntity != null)
            yield return handsComp.ActiveHandEntity.Value;

        foreach (var name in handsComp.SortedHands)
        {
            if (name == handsComp.ActiveHand?.Name)
                continue;

            if (handsComp.Hands[name].HeldEntity is { } held)
                yield return held;
        }
    }

    /// <summary>
    ///     Set the currently active hand and raise hand (de)selection events directed at the held entities.
    /// </summary>
    /// <returns>True if the active hand was set to a NEW value. Setting it to the same value returns false and does
    /// not trigger interactions.</returns>
    public virtual bool TrySetActiveHand(EntityUid uid, string? name, HandsComponent? handComp = null)
    {
        if (!Resolve(uid, ref handComp))
            return false;

        if (name == handComp.ActiveHand?.Name)
            return false;

        Hand? hand = null;
        if (name != null && !handComp.Hands.TryGetValue(name, out hand))
            return false;
        return SetActiveHand(uid, hand, handComp);
    }

    /// <summary>
    ///     Set the currently active hand and raise hand (de)selection events directed at the held entities.
    /// </summary>
    /// <returns>True if the active hand was set to a NEW value. Setting it to the same value returns false and does
    /// not trigger interactions.</returns>
    public bool SetActiveHand(EntityUid uid, Hand? hand, HandsComponent? handComp = null)
    {
        if (!Resolve(uid, ref handComp))
            return false;

        if (hand == handComp.ActiveHand)
            return false;

        if (handComp.ActiveHand?.HeldEntity is { } held)
            RaiseLocalEvent(held, new HandDeselectedEvent(uid));

        if (hand == null)
        {
            handComp.ActiveHand = null;
            return true;
        }

        handComp.ActiveHand = hand;
        OnHandSetActive?.Invoke((uid, handComp));

        if (hand.HeldEntity != null)
            RaiseLocalEvent(hand.HeldEntity.Value, new HandSelectedEvent(uid));

        Dirty(uid, handComp);
        return true;
    }

    public bool IsHolding(Entity<HandsComponent?> entity, [NotNullWhen(true)] EntityUid? item)
    {
        return IsHolding(entity, item, out _, entity);
    }

    public bool IsHolding(EntityUid uid, [NotNullWhen(true)] EntityUid? entity, [NotNullWhen(true)] out Hand? inHand, HandsComponent? handsComp = null)
    {
        inHand = null;
        if (entity == null)
            return false;

        if (!Resolve(uid, ref handsComp, false))
            return false;

        foreach (var hand in handsComp.Hands.Values)
        {
            if (hand.HeldEntity == entity)
            {
                inHand = hand;
                return true;
            }
        }

        return false;
    }

    public bool TryGetHand(EntityUid handsUid, string handId, [NotNullWhen(true)] out Hand? hand,
        HandsComponent? hands = null)
    {
        hand = null;

        if (!Resolve(handsUid, ref hands))
            return false;

        return hands.Hands.TryGetValue(handId, out hand);
    }

    public int CountFreeableHands(Entity<HandsComponent> hands, bool excludeActiveHand = false) // Goob edit
    {
        var freeable = 0;
        foreach (var hand in hands.Comp.Hands.Values)
        {
            if (excludeActiveHand && hands.Comp.ActiveHand != null && hand == hands.Comp.ActiveHand)
                continue;

            if (hand.IsEmpty || CanDropHeld(hands, hand))
                freeable++;
        }

        return freeable;
    }

    /// <summary>
    /// Shitmed Change: This function checks when adding a hand for symmetries to determine where to add it in the sorted hands array.
    /// </summary>
    /// <param name="handsComp">The hands component that we're modifying.</param>
    /// <param name="handName">The name of the hand we're adding.</param>
    /// <param name="handLocation">The location/symmetry of the hand we're adding.</param>
    public virtual void AddToSortedHands(HandsComponent handsComp, string handName, HandLocation handLocation)
    {
        var index = handLocation == HandLocation.Right
            ? 0
            : handLocation == HandLocation.Left
                ? handsComp.SortedHands.Count
                : handsComp.SortedHands.FindIndex(name => handsComp.Hands[name].Location == HandLocation.Right);

        if (index == -1)
            index = handsComp.SortedHands.Count;

        handsComp.SortedHands.Insert(index, handName);
    }
}