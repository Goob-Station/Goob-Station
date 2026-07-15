// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Singularity;

/// <summary>
/// Raised on an entity that just collided with a containment field
/// </summary>
[ByRefEvent]
public record struct ContainmentFieldThrowEvent(EntityUid Field, bool Cancelled = false);
