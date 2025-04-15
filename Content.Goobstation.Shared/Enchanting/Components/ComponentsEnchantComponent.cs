// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ComponentsEnchantComponent))]
[EntityCategory("Enchants")]
public sealed partial class ComponentsEnchantComponent : Component
{
    /// <summary>
    /// Components to add to the item.
    /// </summary>
    [DataField]
    public ComponentRegistry? Added;

    /// <summary>
    /// Components to remove from the item.
    /// </summary>
    [DataField]
    public ComponentRegistry? Removed;
}
