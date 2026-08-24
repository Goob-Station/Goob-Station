// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Chemistry.EntitySystems.Hypospray;

/// <summary>
/// Raised on a hypospray when it successfully injects.
/// </summary>
[ByRefEvent]
public record struct AfterHyposprayInjectsEvent()
{
    /// <summary>
    /// Entity that used the hypospray.
    /// </summary>
    public EntityUid User;

    /// <summary>
    /// Entity that was injected.
    /// </summary>
    public EntityUid Target;
}