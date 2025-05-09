// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Goidastation.Server.OnPray.HealUserOnPray;

[RegisterComponent]
public sealed partial class HealUserOnPrayComponent : Component
{
    // Holy by default.
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            ["Holy"] = -10,
        },
    };
}
