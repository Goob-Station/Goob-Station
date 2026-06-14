// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Server._Funkystation.MalfAI.Disruption;

/// <summary>
/// Added to an RCD that has been armed by the Malf AI detonate ability.
/// Drives the beep-then-explode sequence via MalfAiDetonateRcdsSystem.Update().
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiArmedRcdComponent : Component
{
    /// <summary>Absolute game time of the next beep.</summary>
    [DataField]
    public TimeSpan NextBeepTime;

    /// <summary>Absolute game time when the RCD detonates.</summary>
    [DataField]
    public TimeSpan DetonateTime;

    /// <summary>Interval between warning beeps.</summary>
    [DataField]
    public TimeSpan BeepInterval = TimeSpan.FromSeconds(1);
}
