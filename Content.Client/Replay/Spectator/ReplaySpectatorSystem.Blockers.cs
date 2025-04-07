// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Throwing;

namespace Content.Client.Replay.Spectator;

public sealed partial class ReplaySpectatorSystem
{
    private void InitializeBlockers()
    {
        // Block most interactions to avoid mispredicts
        // This **shouldn't** be required, but just in case.
        SubscribeLocalEvent<ReplaySpectatorComponent, UseAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<ReplaySpectatorComponent, PickupAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<ReplaySpectatorComponent, ThrowAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<ReplaySpectatorComponent, InteractionAttemptEvent>(OnInteractAttempt);
        SubscribeLocalEvent<ReplaySpectatorComponent, AttackAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<ReplaySpectatorComponent, DropAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<ReplaySpectatorComponent, IsEquippingAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<ReplaySpectatorComponent, IsUnequippingAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<ReplaySpectatorComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<ReplaySpectatorComponent, ChangeDirectionAttemptEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<ReplaySpectatorComponent, PullAttemptEvent>(OnPullAttempt);
    }

    private void OnInteractAttempt(Entity<ReplaySpectatorComponent> ent, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnAttempt(EntityUid uid, ReplaySpectatorComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    private void OnUpdateCanMove(EntityUid uid, ReplaySpectatorComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    private void OnPullAttempt(EntityUid uid, ReplaySpectatorComponent component, PullAttemptEvent args)
    {
        args.Cancelled = true;
    }
}
