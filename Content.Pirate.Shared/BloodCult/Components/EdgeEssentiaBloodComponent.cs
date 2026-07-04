// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared.BloodCult.Components;

/// <summary>
/// Component to track the original blood type of an entity affected by Edge Essentia
/// and how much Sanguine Perniculate they've bled for the ritual pool.
/// </summary>
[RegisterComponent]
public sealed partial class EdgeEssentiaBloodComponent : Component
{
    /// <summary>
    /// The original blood reagent before Edge Essentia changed it.
    /// </summary>
    [DataField]
    public string OriginalBloodReagent = "Blood";

    /// <summary>
    /// Tracks the last amount of Sanguine Perniculate in the temporary solution to detect new bleeding.
    /// </summary>
    [DataField]
    public FixedPoint2 LastTrackedBloodAmount = FixedPoint2.Zero;
}
