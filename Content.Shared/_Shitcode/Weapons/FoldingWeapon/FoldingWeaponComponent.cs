// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Weapons.FoldingWeapon;

/// <summary>
/// Prevents shooting and wielding the weapon if it is toggled off
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FoldingWeaponComponent : Component
{
}