// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Goobstation.Server.OnPray.HealNearOnPray;

[RegisterComponent]
public sealed partial class HealNearOnPrayComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new();

    [DataField]
    public List<EntityUid> HealedEntities = new();

    [DataField]
    public int Range = 5;
}
