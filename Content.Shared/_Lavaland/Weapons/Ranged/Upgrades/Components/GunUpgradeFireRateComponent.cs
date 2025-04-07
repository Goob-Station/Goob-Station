// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// A <see cref="GunUpgradeComponent"/> for increasing the firerate of a gun.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class GunUpgradeFireRateComponent : Component
{
    [DataField]
    public float Coefficient = 1;
}