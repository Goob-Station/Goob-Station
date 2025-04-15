// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Randomizes your insulation every time you get shocked.
/// Higher levels make the RNG more favourable.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(BudgetInsulatedEnchantSystem))]
[AutoGenerateComponentState]
[EntityCategory("Enchants")]
public sealed partial class BudgetInsulatedEnchantComponent : Component
{
    /// <summary>
    /// Possible siemens coefficients to pick from.
    /// Duplicate values increase the weight.
    /// Higher levels remove worse coefficients.
    /// </summary>
    [DataField]
    public List<float> Coefficients = new()
    {
        0f,
        0f,
        0.5f,
        0.5f,
        0.5f,
        1.5f,
        1.5f,
        2f,
        2.5f,
        2.5f,
        3f,
        3.5f,
        4f
    };

    /// <summary>
    /// The coefficient used for the next shock.
    /// Networked so if electrocution gets predicted it just werks.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float NextCoefficient;
}
