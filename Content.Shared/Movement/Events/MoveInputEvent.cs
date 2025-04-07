// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared.Movement.Events;

/// <summary>
/// Raised on an entity whenever it has a movement input change.
/// </summary>
[ByRefEvent]
public readonly struct MoveInputEvent
{
    public readonly Entity<InputMoverComponent> Entity;
    public readonly MoveButtons OldMovement;
    public readonly Direction Dir; // Shitmed Change
    public readonly bool State; // Shitmed Change

    public bool HasDirectionalMovement => (Entity.Comp.HeldMoveButtons & MoveButtons.AnyDirection) != MoveButtons.None;

    public MoveInputEvent(Entity<InputMoverComponent> entity, MoveButtons oldMovement, Direction dir, bool state) // Shitmed Change
    {
        Entity = entity;
        OldMovement = oldMovement;
        // Shitmed Change
        Dir = dir;
        State = state;
    }
}