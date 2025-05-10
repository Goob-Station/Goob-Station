// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.MartialArts.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DragonPowerBuffComponent : Component
{
    [DataField]
    public DamageModifierSet ModifierSet = new()
    {
        Coefficients =
        {
            {"Blunt", 0.6f},
            {"Slash", 0.6f},
            {"Piercing", 0.6f},
            {"Heat", 0.6f},
        },
        IgnoreArmorPierceFlags = (int) PartialArmorPierceFlags.All,
    };

    [DataField]
    public float DamageMultiplier = 1.2f;

    [DataField]
    public TimeSpan AttackDamageBuffDuration = TimeSpan.FromSeconds(5);
}
