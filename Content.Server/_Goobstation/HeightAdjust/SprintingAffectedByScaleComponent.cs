// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 MarkerWicker <markerWicker@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.HeightAdjust;

/// <summary>
///     When applied to a humanoid or any mob, adjusts their sprinting speed based on their scale after entity initialization.
///     Smaller characters sprint slower. Larger characters sprint faster.
///     <br/>
///     The formula for the resulting sprint speed is <code>S = SprintSpeedMultiplier * ((xscale + yscale) / 2)</code>
///     clamped between the specified Min and Max values.
/// </summary>
[RegisterComponent]
public sealed partial class SprintingAffectedByScaleComponent : Component
{
    /// <summary>
    ///     Minimum and maximum resulting sprint speed.
    ///     A minimum value of 0.5 means that the resulting sprint speed will be at least 50% of the original.
    /// </summary>
    /// <remarks>
    ///     I've balanced this such that smaller characters take a major penalty, while larger characters might not notice a difference.
    ///     This is because, smaller characters are harder to hit in combat due to sprite clicking.
    ///     This should help solve this combat frustration.
    /// <remarks>
    [DataField]
    public float Min = 0.75f, Max = 1.05f;
}
