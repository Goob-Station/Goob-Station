// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Modifies all incoming damage by <c>factor^level</c>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(DamageModifyEnchantSystem))]
[EntityCategory("Enchants")]
public sealed partial class DamageModifyEnchantComponent : Component
{
    /// <summary>
    /// Base of the exponential function.
    /// Gets repeatedly applied for each level, so 0.5 on level 2 blocks 75% of damage
    /// </summary>
    [DataField(required: true)]
    public float Factor;

    /// <summary>
    /// The modifier to apply to damage, cached when enchant is added and upgraded.
    /// </summary>
    [DataField]
    public float Modifier;

    /// <summary>
    /// Whether to protect someone wearing enchanted clothing instead of the item itself.
    /// </summary>
    [DataField]
    public bool ProtectWearer;
}
