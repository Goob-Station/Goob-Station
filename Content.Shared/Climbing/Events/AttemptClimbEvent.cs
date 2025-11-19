// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Climbing.Events;

[ByRefEvent]
public record struct AttemptClimbEvent(EntityUid User, EntityUid Climber, EntityUid Climbable)
{
    public bool Cancelled;
}
