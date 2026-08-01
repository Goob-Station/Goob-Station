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

// WD EDIT START
/// <summary>
/// Raised on an entity whenever it has a sprinting input change.
/// </summary>
public readonly struct SprintingInputEvent(Entity<InputMoverComponent> entity)
{
    public readonly Entity<InputMoverComponent> Entity = entity;
}
// WD EDIT END
