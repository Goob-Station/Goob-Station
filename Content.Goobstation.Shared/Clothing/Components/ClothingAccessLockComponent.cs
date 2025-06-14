// SPDX-FileCopyrightText: 2025 Coenx-flex <coengmurray@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Clothing.Components;

/// <summary>
/// Requires access to take clothing on and off
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ClothingAccessLockComponent : Component
{
    /// <summary>
    /// Do you need access to put the clothing on?
    /// </summary>
    [DataField]
    public bool RequireEquip;

    /// <summary>
    /// Do you need access to take the clothing off?
    /// </summary>
    [DataField]
    public bool RequireUnequip = true;
}
