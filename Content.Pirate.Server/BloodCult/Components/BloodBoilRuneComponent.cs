// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.BloodCult.Components;

[RegisterComponent]
public sealed partial class BloodBoilRuneComponent : Component
{
    [DataField]
    public EntProtoId ProjectilePrototype = "ProjectileBloodBoil";

    [DataField]
    public float ProjectileSpeed = 50f;

    [DataField]
    public float TargetRange = 15f;

    [DataField]
    public int ProjectileCount = 3;

    [DataField]
    public float FireStacksPerProjectile = 1f;

    [DataField]
    public float InvokerRange = 1.5f;

    [DataField]
    public int RequiredInvokers = 3;

    [DataField]
    public DamageSpecifier InvocationDamage = new()
    {
        DamageDict = new()
        {
            { "Slash", 35 },
        },
    };

    [DataField]
    public SoundSpecifier ActivationSound = new SoundPathSpecifier("/Audio/_Pirate/BloodCult/magic.ogg");
}
