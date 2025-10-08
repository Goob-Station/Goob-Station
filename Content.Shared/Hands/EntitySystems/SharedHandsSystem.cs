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
using Robust.Shared.Utility;

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

    public event Action<Entity<HandsComponent>, string, HandLocation>? OnPlayerAddHand;
    public event Action<Entity<HandsComponent>, string>? OnPlayerRemoveHand;
    protected event Action<Entity<HandsComponent>?>? OnHandSetActive;

    public override void Initialize()
    {
        base.Initialize();

        InitializeInteractions();
        InitializeDrop();
        InitializePickup();
        InitializeRelay();

        SubscribeLocalEvent<HandsComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<HandsComponent, MapInitEvent>(OnMapInit);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        CommandBinds.Unregister<SharedHandsSystem>();
    }

    private void OnInit(Entity<HandsComponent> ent, ref ComponentInit args)
    {
        var container = EnsureComp<ContainerManagerComponent>(ent);
        foreach (var id in ent.Comp.Hands.Keys)
        {
            ContainerSystem.EnsureContainer<ContainerSlot>(ent, id, container);
        }
    }

    private void OnMapInit(Entity<HandsComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.ActiveHandId == null)
            SetActiveHand(ent.AsNullable(), ent.Comp.SortedHands.FirstOrDefault());
    }

    /// <summary>
    /// Adds a hand with the given container id and supplied location to the specified entity.
    /// </summary>
    public void AddHand(Entity<HandsComponent?> ent, string handName, HandLocation handLocation)
    {
        AddHand(ent, handName, new Hand(handLocation));
    }

    /// <summary>
    /// Adds a hand with the given container id and supplied hand definition to the given entity.
    /// </summary>
    public void AddHand(Entity<HandsComponent?> ent, string handName, Hand hand)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (ent.Comp.Hands.ContainsKey(handName))
            return;

        var container = ContainerSystem.EnsureContainer<ContainerSlot>(ent, handName);
        container.OccludesLight = false;

        ent.Comp.Hands.Add(handName, hand);
        ent.Comp.SortedHands.Add(handName);
        Dirty(ent);

        OnPlayerAddHand?.Invoke((ent, ent.Comp), handName, hand.Location);

        if (ent.Comp.ActiveHandId == null)
            SetActiveHand(ent, handName);

        RaiseLocalEvent(ent, new HandCountChangedEvent(ent));
    }

    /// <summary>
    /// Removes the specified hand from the specified entity
    /// </summary>
    public virtual void RemoveHand(Entity<HandsComponent?> ent, string handName)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        OnPlayerRemoveHand?.Invoke((ent, ent.Comp), handName);

        TryDrop(ent, handName, null, false);

        if (!ent.Comp.Hands.Remove(handName))
            return;

        if (ContainerSystem.TryGetContainer(ent, handName, out var container))
            ContainerSystem.ShutdownContainer(container);

        ent.Comp.SortedHands.Remove(handName);
        if (ent.Comp.ActiveHandId == handName)
            TrySetActiveHand(ent, ent.Comp.SortedHands.FirstOrDefault());

        RaiseLocalEvent(ent, new HandCountChangedEvent(ent));
        Dirty(ent);
    }

    /// <summary>
    /// Gets rid of all the entity's hands.
    /// </summary>
    public void RemoveHands(Entity<HandsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        var handIds = new List<string>(ent.Comp.Hands.Keys);
        foreach (var handId in handIds)
        {
            RemoveHand(ent, handId);
        }
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
    public bool TryGetEmptyHand(Entity<HandsComponent?> ent, [NotNullWhen(true)] out string? emptyHand)
    {
        emptyHand = null;
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        foreach (var hand in EnumerateHands(ent))
        {
            if (HandIsEmpty(ent, hand))
            {
                emptyHand = hand;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to retrieve the item held in the entity's active hand.
    /// </summary>
    public bool TryGetActiveItem(Entity<HandsComponent?> entity, [NotNullWhen(true)] out EntityUid? item)
    {
        item = null;
        if (!Resolve(entity, ref entity.Comp, false))
            return false;

        if (!TryGetHeldItem(entity, entity.Comp.ActiveHandId, out var held))
            return false;

        item = held;
        return true;
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

    /// <summary>
    /// Gets the current active hand's Id for the specified entity
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public string? GetActiveHand(Entity<HandsComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return null;

        return entity.Comp.ActiveHandId;
    }

    /// <summary>
    /// Gets the current active hand's held entity for the specified entity
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public EntityUid? GetActiveItem(Entity<HandsComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return null;

        return GetHeldItem(entity, entity.Comp.ActiveHandId);
    }

    public bool ActiveHandIsEmpty(Entity<HandsComponent?> entity)
    {
        return GetActiveItem(entity) == null;
    }

    /// <summary>
    ///     Enumerate over hands, starting with the currently active hand.
    /// </summary>
    public IEnumerable<string> EnumerateHands(Entity<HandsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            yield break;

        if (ent.Comp.ActiveHandId != null)
            yield return ent.Comp.ActiveHandId;

        foreach (var name in ent.Comp.SortedHands)
        {
            if (name != ent.Comp.ActiveHandId)
                yield return name;
        }
    }

    /// <summary>
    ///     Enumerate over held items, starting with the item in the currently active hand (if there is one).
    /// </summary>
    public IEnumerable<EntityUid> EnumerateHeld(Entity<HandsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            yield break;

        if (TryGetActiveItem(ent, out var activeHeld))
            yield return activeHeld.Value;

        foreach (var name in ent.Comp.SortedHands)
        {
            if (name == ent.Comp.ActiveHandId)
                continue;

            if (TryGetHeldItem(ent, name, out var held))
                yield return held.Value;
        }
    }

    /// <summary>
    ///     Set the currently active hand and raise hand (de)selection events directed at the held entities.
    /// </summary>
    /// <returns>True if the active hand was set to a NEW value. Setting it to the same value returns false and does
    /// not trigger interactions.</returns>
    public bool TrySetActiveHand(Entity<HandsComponent?> ent, string? name)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (name == ent.Comp.ActiveHandId)
            return false;

        if (name != null && !ent.Comp.Hands.ContainsKey(name))
            return false;
        return SetActiveHand(ent, name);
    }

    /// <summary>
    ///     Set the currently active hand and raise hand (de)selection events directed at the held entities.
    /// </summary>
    /// <returns>True if the active hand was set to a NEW value. Setting it to the same value returns false and does
    /// not trigger interactions.</returns>
    public bool SetActiveHand(Entity<HandsComponent?> ent, string? handId)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (handId == ent.Comp.ActiveHandId)
            return false;

        if (TryGetActiveItem(ent, out var oldHeld))
            RaiseLocalEvent(oldHeld.Value, new HandDeselectedEvent(ent));

        if (handId == null)
        {
            ent.Comp.ActiveHandId = null;
            return true;
        }

        ent.Comp.ActiveHandId = handId;
        OnHandSetActive?.Invoke((ent, ent.Comp));

        if (TryGetHeldItem(ent, handId, out var newHeld))
            RaiseLocalEvent(newHeld.Value, new HandSelectedEvent(ent));

        Dirty(ent);
        return true;
    }

    public bool IsHolding(Entity<HandsComponent?> entity, [NotNullWhen(true)] EntityUid? item)
    {
        return IsHolding(entity, item, out _);
    }

    public bool IsHolding(Entity<HandsComponent?> ent, [NotNullWhen(true)] EntityUid? entity, [NotNullWhen(true)] out string? inHand)
    {
        inHand = null;
        if (entity == null)
            return false;

        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        foreach (var hand in ent.Comp.Hands.Keys)
        {
            if (GetHeldItem(ent, hand) == entity)
            {
                inHand = hand;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to retrieve the associated hand struct corresponding to a hand ID on a given entity.
    /// </summary>
    public bool TryGetHand(Entity<HandsComponent?> ent, [NotNullWhen(true)] string? handId, [NotNullWhen(true)] out Hand? hand)
    {
        hand = null;

        if (handId == null)
            return false;

        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!ent.Comp.Hands.TryGetValue(handId, out var handsHand))
            return false;

        hand = handsHand;
        return true;
    }

    /// <summary>
    /// Gets the item currently held in the entity's specified hand. Returns null if no hands are present or there is no item.
    /// </summary>
    public EntityUid? GetHeldItem(Entity<HandsComponent?> ent, string? handId)
    {
        TryGetHeldItem(ent, handId, out var held);
        return held;
    }

    /// <summary>
    /// Gets the item currently held in the entity's specified hand. Returns false if no hands are present or there is no item.
    /// </summary>
    public bool TryGetHeldItem(Entity<HandsComponent?> ent, string? handId, [NotNullWhen(true)] out EntityUid? held)
    {
        held = null;
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        // Sanity check to make sure this is actually a hand.
        if (handId == null || !ent.Comp.Hands.ContainsKey(handId))
            return false;

        if (!ContainerSystem.TryGetContainer(ent, handId, out var container))
            return false;

        held = container.ContainedEntities.FirstOrNull();
        return held != null;
    }

    public bool HandIsEmpty(Entity<HandsComponent?> ent, string handId)
    {
        return GetHeldItem(ent, handId) == null;
    }

    public int GetHandCount(Entity<HandsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return 0;

        return ent.Comp.Hands.Count;
    }

    public int CountFreeHands(Entity<HandsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return 0;

        var free = 0;
        foreach (var name in ent.Comp.Hands.Keys)
        {
            if (HandIsEmpty(ent, name))
                free++;
        }

        return free;
    }

    public int CountFreeableHands(Entity<HandsComponent> hands, bool excludeActiveHand = false) // Goob edit
    {
        var freeable = 0;
        foreach (var name in hands.Comp.Hands.Keys)
        {
            if (HandIsEmpty(hands.AsNullable(), name) || CanDropHeld(hands, name))
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