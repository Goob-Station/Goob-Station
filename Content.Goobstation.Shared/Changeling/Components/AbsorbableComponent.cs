// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
///     Component that indicates that a person can be absorbed by a changeling.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AbsorbableComponent : Component
{
    /// <summary>
    /// Should absorb of this entity progress changeling objective
    /// </summary>
    [DataField]
    public bool ProgressObjective = true;

    /// <summary>
    /// How much evolution points absorbation of this entity give
    /// </summary>
    [DataField]
    public float EvolutionPoints = 2f;

    /// <summary>
    /// How much additional chemical adds to max chemicals limit
    /// </summary>
    [DataField]
    public float BonusChemicals = 10f;

    /// <summary>
    /// Is this entity absorbed already?
    /// </summary>
    [ViewVariables]
    public bool Absorbed = false;
}
