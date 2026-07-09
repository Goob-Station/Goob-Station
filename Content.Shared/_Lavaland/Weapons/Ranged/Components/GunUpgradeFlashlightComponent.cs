// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using Content.Shared.Actions.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Ranged.Components;

/// <summary>
///     Component to indicate a valid flashlight for weapon attachment
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GunUpgradeFlashlightComponent : Component
{
    /// <summary>
    /// Original action icon style before this flashlight was attached to a gun.
    /// </summary>
    public ItemActionIconStyle OriginalItemIconStyle = ItemActionIconStyle.NoItem;

    /// <summary>
    /// Original use delay before this flashlight was attached to a gun.
    /// </summary>
    public TimeSpan? OriginalUseDelay;

    /// <summary>
    /// True after original action visual/use settings were captured on insert.
    /// </summary>
    public bool HasSavedActionDefaults;
}
