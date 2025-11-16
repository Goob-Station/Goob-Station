// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Fax;

/// <summary>
/// Causes items to, instead of their normal fax behavior, have a chance to teleport to the fax destination instead.
/// Can have a separate chance for lubed items.
/// </summary>
[RegisterComponent]
public sealed partial class FaxSlipComponent : Component
{
    /// <summary>
    /// Chance we get teleported to destination instead of normal behavior.
    /// </summary>
    [DataField]
    public float SlipChance = 0.3f;

    /// <summary>
    /// If not null, chance override for lubed items.
    /// Also will also allow insertion of this item into faxes even if lubed.
    /// </summary>
    [DataField]
    public float? LubedChance = 1f;

    /// <summary>
    /// Whether to work if the destination and source faxes are on different grids.
    /// </summary>
    [DataField]
    public bool CrossGrid = true;
}
