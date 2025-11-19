// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Gravity
{
    [ByRefEvent]
    public readonly record  struct GravityChangedEvent(EntityUid ChangedGridIndex, bool HasGravity);
}
