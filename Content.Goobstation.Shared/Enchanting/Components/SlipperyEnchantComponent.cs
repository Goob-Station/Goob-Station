// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Scales <c>SlipperyComponent</c> values by the enchant level.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SlipperyEnchantSystem))]
[EntityCategory("Enchants")]
public sealed partial class SlipperyEnchantComponent : Component
{
    /// <summary>
    /// The base modifier to use with level 1.
    /// This makes it so enchanting soap etc is still good
    /// </summary>
    [DataField]
    public float BaseModifier = 1.25f;
}
