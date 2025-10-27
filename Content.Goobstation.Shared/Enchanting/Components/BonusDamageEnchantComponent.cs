// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Systems;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Linearly adds bonus damage to melee attacks with <c>damage * level</c>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(BonusDamageEnchantSystem))]
[AutoGenerateComponentState]
[EntityCategory("Enchants")]
public sealed partial class BonusDamageEnchantComponent : Component
{
    /// <summary>
    /// Bonus damage to apply to melee attacks.
    /// Gets multiplied by level.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public DamageSpecifier Damage = new();
}
