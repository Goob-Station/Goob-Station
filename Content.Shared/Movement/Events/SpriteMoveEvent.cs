// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Movement.Events;

/// <summary>
/// Raised on an entity whenever it should change movement sprite
/// </summary>
[ByRefEvent]
public readonly struct SpriteMoveEvent
{
    public readonly bool IsMoving = false;

    public SpriteMoveEvent(bool isMoving)
    {
        IsMoving = isMoving;
    }
}
