// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Climbing.Events;

[ByRefEvent]
public record struct AttemptClimbEvent(EntityUid User, EntityUid Climber, EntityUid Climbable)
{
    public bool Cancelled;
}