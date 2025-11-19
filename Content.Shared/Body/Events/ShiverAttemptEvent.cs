// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Body.Events;

[ByRefEvent]
public record struct ShiverAttemptEvent(EntityUid Uid)
{
    public readonly EntityUid Uid = Uid;
    public bool Cancelled = false;
}
