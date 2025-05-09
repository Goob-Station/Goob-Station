// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goidastation.Shared.Weapons.Multihit;

[RegisterComponent]
public sealed partial class ActiveMultihitComponent : Component
{
    [ViewVariables]
    public float DamageMultiplier = 1f;
}
