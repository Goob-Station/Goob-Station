// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 MarkerWicker <markerWicker@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.HeightAdjust;

/// <summary>
///     When applied to a humanoid or any mob with a FlightComponent (harpies mostly), adjusts their flight speed and stamina drain based on their scale after entity initialization.
///     Smaller harpies will fly slower, but can fly for longer. Larger harpies fly faster, but can't fly so long.
///     <br/>
///     The formula for the resulting flight speed is <code>S = SprintSpeedMultiplier * ((xscale + yscale) / 2)</code>
///     clamped between the specified Min and Max values.
/// </summary>
/// <remarks>
///     Notice that the minimum and maximum values are effectively swapped for the speed factor and stamina drain factor.
///     This is balanced to discourage disproportionate harpies. Flight is an EXTREMELY potent movement tool, and could become incredibly frustrating
///     when combined with a small character (difficult to hit) or a large character (potent in combat).
///     Small harpies get an insignificant buff to their stamina drain as solace, and the same is true with large harpies and their speed buff.
/// </remarks>
[RegisterComponent]
public sealed partial class FlightAffectedByScaleComponent : Component
{
    /// <summary>
    ///     Minimum and maximum resulting flight speed.
    ///     A minimum value of 0.5 means that the resulting flight speed will be at least 50% of the original.
    /// </summary>
    [DataField]
    public float MinSpeedFactor = 0.75f, MaxSpeedFactor = 1.05f;

    /// <summary>
    ///     Minimum and maximum resulting stamia drain.
    ///     A maximum value of 1.5 means that the resulting stamina drain can go up to 150% of the original.
    /// </summary>
    [DataField]
    public float MinStaminaDrainFactor = 0.95f, MaxStaminaDrainFactor = 1.25f;
}
