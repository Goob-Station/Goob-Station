// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Weapons.CounterattackWeapon;

[RegisterComponent]
public sealed partial class CounterattackWeaponUserComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> Weapons = [];
}
