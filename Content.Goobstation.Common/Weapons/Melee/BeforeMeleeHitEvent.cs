// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Melee;

[ByRefEvent]
public sealed class BeforeMeleeHitEvent : HandledEntityEventArgs
{
    public readonly EntityUid Weapon;
    public readonly EntityUid User;
    public readonly float TotalDamage; // goida (i love circular dependencies)
    public readonly bool Heavy;

    public BeforeMeleeHitEvent(EntityUid weapon, EntityUid user, float damage, bool isHeavy = false)
    {
        Weapon = weapon;
        User = user;
        TotalDamage = damage;
        Heavy = isHeavy;
    }
}
