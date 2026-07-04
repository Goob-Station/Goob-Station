// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Components;

namespace Content.Shared.Movement.Events;

/// <summary>
///     Raised whenever <see cref="InputMoverComponent.CanMove"/> needs to be updated.
///     Cancel this event to prevent a mover from moving.
/// </summary>
/// <remarks>
///     This is not an attempt event and the result is cached.
///     If you subscribe to this you must also call <see cref="ActionBlockerSystem.UpdateCanMove(EntityUid,InputMoverComponent?)"/>
///     both when you want to prevent a mob from moving, and when you want to allow them to move again!
/// </remarks>
public sealed class UpdateCanMoveEvent : CancellableEntityEventArgs
{
    public UpdateCanMoveEvent(EntityUid uid)
    {
        Uid = uid;
    }

    public EntityUid Uid { get; }
}
