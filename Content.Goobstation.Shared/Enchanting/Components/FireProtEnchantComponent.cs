// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Linearly reduces damage from firestacks by <c>factor * level</c>.
/// Similar to <c>FireProtectionComponent</c> for clothing but networked.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(FireProtEnchantSystem))]
[AutoGenerateComponentState]
[EntityCategory("Enchants")]
public sealed partial class FireProtEnchantComponent : Component
{
    /// <summary>
    /// Base of the linear fire protection function.
    /// Gets repeatedly added for each level then subtracted from the fire modifier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Reduction = 0.1f;

    /// <summary>
    /// Base of the exponential heat protection function.
    /// Gets exponentially smaller with higher levels, eventually making it nigh-impossible to heat you up.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float TempModifier = 0.1f;
}
