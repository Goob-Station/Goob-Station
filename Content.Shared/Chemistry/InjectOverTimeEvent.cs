// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Chemistry.Events;

/// <summary>
/// Raised directed on an entity when it embeds in another entity.
/// </summary>
[ByRefEvent]
public readonly record struct InjectOverTimeEvent(EntityUid embeddedIntoUid)
{
    /// <summary>
    /// Entity that is embedded in.
    /// </summary>
    public readonly EntityUid EmbeddedIntoUid = embeddedIntoUid;
}

// Goobstation
[ByRefEvent]
public record struct InjectOverTimeAttemptEvent(EntityUid EmbeddedIntoUid, bool Cancelled = false);
