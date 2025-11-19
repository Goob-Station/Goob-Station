// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Movement.Events
{
    /// <summary>
    /// Raised on an entity's parent when it has movement inputs while in a container.
    /// </summary>
    [ByRefEvent]
    public readonly struct ContainerRelayMovementEntityEvent
    {
        public readonly EntityUid Entity;

        public ContainerRelayMovementEntityEvent(EntityUid entity)
        {
            Entity = entity;
        }
    }
}
