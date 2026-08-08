// SPDX-FileCopyrightText: 2026 Impstation contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Changeling;

/// <summary>
/// Component that indicates that an entity can be absorbed by a changeling in Impstation content.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GoobAbsorbableComponent : Component
{
    [DataField("disabled")]
    public bool Disabled;

    /// <summary>
    /// Percentage of biomass restored on consumption.
    /// </summary>
    [DataField]
    public float BiomassRestored = 1f;
}
