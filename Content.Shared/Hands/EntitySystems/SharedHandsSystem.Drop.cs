// SPDX-FileCopyrightText: 2022 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Menshin <Menshin@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 mokiros <Mokiros@yandex.ru>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Geekyhobo <66805063+Geekyhobo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <shadowjjt@gmail.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Token <esil.bektay@yandex.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Database;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Tag;
using Robust.Shared.Containers;
using Robust.Shared.Map;

namespace Content.Shared.Hands.EntitySystems;

public abstract partial class SharedHandsSystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    private void InitializeDrop()
    {
        SubscribeLocalEvent<HandsComponent, EntRemovedFromContainerMessage>(HandleEntityRemoved);
    }

    protected virtual void HandleEntityRemoved(EntityUid uid, HandsComponent hands, EntRemovedFromContainerMessage args)
    {
        if (!TryGetHand(uid, args.Container.ID, out var hand))
        {
            return;
        }

        var gotUnequipped = new GotUnequippedHandEvent(uid, args.Entity, hand);
        RaiseLocalEvent(args.Entity, gotUnequipped);

        var didUnequip = new DidUnequipHandEvent(uid, args.Entity, hand);
        RaiseLocalEvent(uid, didUnequip);

        if (TryComp(args.Entity, out VirtualItemComponent? @virtual))
            _virtualSystem.DeleteVirtualItem((args.Entity, @virtual), uid);
    }

    private bool ShouldIgnoreRestrictions(EntityUid user)
    {
        //Checks if the Entity is something that shouldn't care about drop distance or walls ie Aghost
        return !_tagSystem.HasTag(user, "BypassDropChecks");
    }

    /// <summary>
    ///     Checks whether an entity can drop a given entity. Will return false if they are not holding the entity.
    /// </summary>
    public bool CanDrop(EntityUid uid, EntityUid entity, HandsComponent? handsComp = null, bool checkActionBlocker = true)
    {
        if (!Resolve(uid, ref handsComp))
            return false;

        if (!IsHolding(uid, entity, out var hand, handsComp))
            return false;

        return CanDropHeld(uid, hand, checkActionBlocker);
    }

    /// <summary>
    ///     Checks if the contents of a hand is able to be removed from its container.
    /// </summary>
    public bool CanDropHeld(EntityUid uid, Hand hand, bool checkActionBlocker = true)
    {
        if (hand.Container?.ContainedEntity is not {} held)
            return false;

        if (!ContainerSystem.CanRemove(held, hand.Container))
            return false;

        if (checkActionBlocker && !_actionBlocker.CanDrop(uid))
            return false;

        return true;
    }

    /// <summary>
    ///     Attempts to drop the item in the currently active hand.
    /// </summary>
    public bool TryDrop(EntityUid uid, EntityCoordinates? targetDropLocation = null, bool checkActionBlocker = true, bool doDropInteraction = true, HandsComponent? handsComp = null)
    {
        if (!Resolve(uid, ref handsComp))
            return false;

        if (handsComp.ActiveHand == null)
            return false;

        return TryDrop(uid, handsComp.ActiveHand, targetDropLocation, checkActionBlocker, doDropInteraction, handsComp);
    }

    /// <summary>
    ///     Drops an item at the target location.
    /// </summary>
    public bool TryDrop(EntityUid uid, EntityUid entity, EntityCoordinates? targetDropLocation = null, bool checkActionBlocker = true, bool doDropInteraction = true, HandsComponent? handsComp = null)
    {
        if (!Resolve(uid, ref handsComp))
            return false;

        if (!IsHolding(uid, entity, out var hand, handsComp))
            return false;

        return TryDrop(uid, hand, targetDropLocation, checkActionBlocker, doDropInteraction, handsComp);
    }

    /// <summary>
    ///     Drops a hands contents at the target location.
    /// </summary>
    public bool TryDrop(EntityUid uid, Hand hand, EntityCoordinates? targetDropLocation = null, bool checkActionBlocker = true, bool doDropInteraction = true, HandsComponent? handsComp = null)
    {
        if (!Resolve(uid, ref handsComp))
            return false;

        if (!CanDropHeld(uid, hand, checkActionBlocker))
            return false;

        var entity = hand.HeldEntity!.Value;

        // if item is a fake item (like with pulling), just delete it rather than bothering with trying to drop it into the world
        if (TryComp(entity, out VirtualItemComponent? @virtual))
            _virtualSystem.DeleteVirtualItem((entity, @virtual), uid);

        if (TerminatingOrDeleted(entity))
            return true;

        var itemXform = Transform(entity);
        if (itemXform.MapUid == null)
            return true;

        var userXform = Transform(uid);
        var isInContainer = ContainerSystem.IsEntityOrParentInContainer(uid, xform: userXform);

        // if the user is in a container, drop the item inside the container
        if (isInContainer) {
            TransformSystem.DropNextTo((entity, itemXform), (uid, userXform));
            return true;
        }

        // drop the item with heavy calculations from their hands and place it at the calculated interaction range position
        // The DoDrop is handle if there's no drop target
        DoDrop(uid, hand, doDropInteraction: doDropInteraction, handsComp);

        // if there's no drop location stop here
        if (targetDropLocation == null)
            return true;

        // otherwise, also move dropped item and rotate it properly according to grid/map
        var (itemPos, itemRot) = TransformSystem.GetWorldPositionRotation(entity);
        var origin = new MapCoordinates(itemPos, itemXform.MapID);
        var target = TransformSystem.ToMapCoordinates(targetDropLocation.Value);
        TransformSystem.SetWorldPositionRotation(entity, GetFinalDropCoordinates(uid, origin, target, entity), itemRot);
        return true;
    }

    /// <summary>
    ///     Attempts to move a held item from a hand into a container that is not another hand, without dropping it on the floor in-between.
    /// </summary>
    public bool TryDropIntoContainer(EntityUid uid, EntityUid entity, BaseContainer targetContainer, bool checkActionBlocker = true, HandsComponent? handsComp = null)
    {
        if (!Resolve(uid, ref handsComp))
            return false;

        if (!IsHolding(uid, entity, out var hand, handsComp))
            return false;

        if (!CanDropHeld(uid, hand, checkActionBlocker))
            return false;

        if (!ContainerSystem.CanInsert(entity, targetContainer))
            return false;

        DoDrop(uid, hand, false, handsComp);
        ContainerSystem.Insert(entity, targetContainer);
        return true;
    }

    /// <summary>
    ///     Calculates the final location a dropped item will end up at, accounting for max drop range and collision along the targeted drop path, Does a check to see if a user should bypass those checks as well.
    /// </summary>
    private Vector2 GetFinalDropCoordinates(EntityUid user, MapCoordinates origin, MapCoordinates target, EntityUid held)
    {
        var dropVector = target.Position - origin.Position;
        var requestedDropDistance = dropVector.Length();
        var dropLength = dropVector.Length();

        if (ShouldIgnoreRestrictions(user))
        {
            if (dropVector.Length() > SharedInteractionSystem.InteractionRange)
            {
                dropVector = dropVector.Normalized() * SharedInteractionSystem.InteractionRange;
                target = new MapCoordinates(origin.Position + dropVector, target.MapId);
            }

            dropLength = _interactionSystem.UnobstructedDistance(origin, target, predicate: e => e == user || e == held);
        }

        if (dropLength < requestedDropDistance)
            return origin.Position + dropVector.Normalized() * dropLength;
        return target.Position;
    }

    /// <summary>
    ///     Removes the contents of a hand from its container. Assumes that the removal is allowed. In general, you should not be calling this directly.
    /// </summary>
    public virtual void DoDrop(EntityUid uid, Hand hand, bool doDropInteraction = true, HandsComponent? handsComp = null, bool log = true)
    {
        if (!Resolve(uid, ref handsComp))
            return;

        if (hand.Container?.ContainedEntity == null)
            return;

        var entity = hand.Container.ContainedEntity.Value;

        if (TerminatingOrDeleted(uid) || TerminatingOrDeleted(entity))
            return;

        if (!ContainerSystem.Remove(entity, hand.Container))
        {
            Log.Error($"Failed to remove {ToPrettyString(entity)} from users hand container when dropping. User: {ToPrettyString(uid)}. Hand: {hand.Name}.");
            return;
        }

        Dirty(uid, handsComp);

        if (doDropInteraction)
            _interactionSystem.DroppedInteraction(uid, entity);

        if (log)
            _adminLogger.Add(LogType.Drop, LogImpact.Low, $"{ToPrettyString(uid):user} dropped {ToPrettyString(entity):entity}");

        if (hand == handsComp.ActiveHand)
            RaiseLocalEvent(entity, new HandDeselectedEvent(uid));
    }
}