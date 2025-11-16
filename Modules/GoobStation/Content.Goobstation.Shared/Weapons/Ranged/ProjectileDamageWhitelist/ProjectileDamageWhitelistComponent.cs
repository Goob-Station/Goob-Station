// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Weapons.Ranged.ProjectileDamageWhitelist;

/// <summary>
/// If a projectile with this component collides with an entity that meets the whitelist, applies the damage.
/// </summary>
[RegisterComponent]
public sealed partial class ProjectileDamageWhitelistComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new ();

    [DataField]
    public bool IgnoreResistances;

    [DataField]
    public EntityWhitelist Whitelist = new ();
}
