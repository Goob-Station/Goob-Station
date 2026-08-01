// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Lavaland.Weapons;

/// <summary>
/// Marker component that used for weapons.
/// If weapon has this component, Megafauna can drop special loot.
/// </summary>
[RegisterComponent]
public sealed partial class MegafaunaWeaponLooterComponent : Component;
