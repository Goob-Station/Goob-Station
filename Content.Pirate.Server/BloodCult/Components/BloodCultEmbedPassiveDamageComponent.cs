// SPDX-FileCopyrightText: 2024 White Dream Project contributors
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Server.BloodCult.Components;

/// <summary>
/// Passively damages the mob that this blood cult weapon is embedded in.
/// </summary>
[RegisterComponent]
public sealed partial class BloodCultEmbedPassiveDamageComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new();

    [DataField]
    public TimeSpan DamageInterval = TimeSpan.FromSeconds(1);

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Embedded;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextDamage;
}
