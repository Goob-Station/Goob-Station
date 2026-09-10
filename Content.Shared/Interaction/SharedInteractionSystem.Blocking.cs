// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands;
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;

namespace Content.Shared.Interaction;

// TODO deduplicate with AdminFrozenComponent
/// <summary>
/// Handles <see cref="BlockMovementComponent"/>, which prevents various
/// kinds of movement and interactions when attached to an entity.
/// </summary>
public partial class SharedInteractionSystem
{
    private void InitializeBlocking()
    {
        SubscribeLocalEvent<BlockMovementComponent, UpdateCanMoveEvent>(OnMoveAttempt);
        SubscribeLocalEvent<BlockMovementComponent, UseAttemptEvent>(CancelUseEvent);
        SubscribeLocalEvent<BlockMovementComponent, InteractionAttemptEvent>(CancelInteractEvent);
        SubscribeLocalEvent<BlockMovementComponent, DropAttemptEvent>(CancellableInteractEvent);
        SubscribeLocalEvent<BlockMovementComponent, PickupAttemptEvent>(CancellableInteractEvent);
        SubscribeLocalEvent<BlockMovementComponent, ChangeDirectionAttemptEvent>(CancelEvent);

        SubscribeLocalEvent<BlockMovementComponent, ComponentStartup>(OnBlockingStartup);
        SubscribeLocalEvent<BlockMovementComponent, ComponentShutdown>(OnBlockingShutdown);

        SubscribeLocalEvent<BlockMovementComponent, ComponentRemove>(OnBlockingRemove);
    }

    private void CancelInteractEvent(Entity<BlockMovementComponent> ent, ref InteractionAttemptEvent args)
    {
        if (ent.Comp.BlockInteraction)
            args.Cancelled = true;
    }

    private void CancelUseEvent(Entity<BlockMovementComponent> ent, ref UseAttemptEvent args)
    {
        if (ent.Comp.BlockUse)
            args.Cancel();
    }

    private void OnMoveAttempt(EntityUid uid, BlockMovementComponent component, UpdateCanMoveEvent args)
    {
        // If we're relaying then don't cancel.
        if (HasComp<RelayInputMoverComponent>(uid))
            return;

        args.Cancel(); // no more scurrying around
    }

    private void CancellableInteractEvent(EntityUid uid, BlockMovementComponent component, CancellableEntityEventArgs args)
    {
        if (component.BlockInteraction)
            args.Cancel();
    }

    private void CancelEvent(EntityUid uid, BlockMovementComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    private void OnBlockingStartup(EntityUid uid, BlockMovementComponent component, ComponentStartup args)
    {
        _actionBlockerSystem.UpdateCanMove(uid);
    }

    private void OnBlockingShutdown(EntityUid uid, BlockMovementComponent component, ComponentShutdown args)
    {
        _actionBlockerSystem.UpdateCanMove(uid);
    }

    // Goob start - fix BlockMovementComponent
    // GOOB TODO: upstream this fix or make a bug report and have someone else do it :trolley:
    // the above still has the comp on the ent along with subscription in this system,
    // so it still updates CanMove to false
    private void OnBlockingRemove(EntityUid uid, BlockMovementComponent component, ComponentRemove args)
    {
        _actionBlockerSystem.UpdateCanMove(uid);
    }
    // Goob end
}
