// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Server._Funkystation.MalfAI.Disruption;

/// <summary>
/// Tracks an active Malf AI grid lockdown so doors can be unbolted when it expires.
/// Added to the AI entity while a lockdown is active.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiLockdownActiveComponent : Component
{
    /// <summary>The grid whose doors are bolted.</summary>
    [DataField]
    public EntityUid Grid;

    /// <summary>Absolute game time when the lockdown expires and doors unbolt.</summary>
    [DataField]
    public TimeSpan EndTime;
}
