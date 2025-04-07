// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.ActionBlocker;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Emoting;
using Content.Shared.Movement.Events;

namespace Content.Shared.Puppet;

// TODO deduplicate with BlockMovementComponent
public abstract class SharedVentriloquistPuppetSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VentriloquistPuppetComponent, UseAttemptEvent>(Cancel);
        SubscribeLocalEvent<VentriloquistPuppetComponent, InteractionAttemptEvent>(CancelInteract);
        SubscribeLocalEvent<VentriloquistPuppetComponent, DropAttemptEvent>(Cancel);
        SubscribeLocalEvent<VentriloquistPuppetComponent, PickupAttemptEvent>(Cancel);
        SubscribeLocalEvent<VentriloquistPuppetComponent, UpdateCanMoveEvent>(Cancel);
        SubscribeLocalEvent<VentriloquistPuppetComponent, EmoteAttemptEvent>(Cancel);
        SubscribeLocalEvent<VentriloquistPuppetComponent, ChangeDirectionAttemptEvent>(Cancel);
        SubscribeLocalEvent<VentriloquistPuppetComponent, ComponentStartup>(OnStartup);
    }

    private void CancelInteract(Entity<VentriloquistPuppetComponent> ent, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnStartup(EntityUid uid, VentriloquistPuppetComponent component, ComponentStartup args)
    {
        _blocker.UpdateCanMove(uid);
    }

    private void Cancel<T>(EntityUid uid, VentriloquistPuppetComponent component, T args) where T : CancellableEntityEventArgs
    {
        args.Cancel();
    }
}