// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.NPC.Queries.Considerations;

/// <summary>
/// Returns if the target is below a certain temperature.
/// </summary>
public sealed partial class TargetLowTempCon : UtilityConsideration
{
    /// <summary>
    /// The minimum temperature they must be.
    /// </summary>
    [DataField]
    public float MinTemp;
}

