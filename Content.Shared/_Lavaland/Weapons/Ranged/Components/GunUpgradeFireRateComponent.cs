// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Ranged.Components;

/// <summary>
/// An upgrade for increasing the firerate of a gun.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GunUpgradeFireRateComponent : Component
{
    [DataField]
    public float Coefficient = 1;
}
