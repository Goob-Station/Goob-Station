// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;

namespace Content.Server.BloodCult.Components;

[RegisterComponent]
public sealed partial class BloodCultWhetstoneComponent : Component
{
    [DataField]
    public int Uses = 1;

    [DataField]
    public DamageSpecifier DamageIncrease = new()
    {
        DamageDict = new()
        {
            { "Slash", 4 },
        },
    };

    [DataField]
    public float MaximumIncrease = 25f;

    [DataField]
    public EntityWhitelist Whitelist = new();

    [DataField]
    public EntityWhitelist Blacklist = new();

    [DataField]
    public SoundSpecifier SharpenSound = new SoundPathSpecifier("/Audio/Items/sheath.ogg");
}
