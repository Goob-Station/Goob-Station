// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Climbing.Events;

/// <summary>
///     Raised on an entity when it successfully climbs on something.
/// </summary>
[ByRefEvent]
public readonly record struct StartClimbEvent(EntityUid Climbable);