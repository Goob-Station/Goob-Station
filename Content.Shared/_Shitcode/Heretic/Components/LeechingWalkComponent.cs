// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent]
public sealed partial class LeechingWalkComponent : Component
{
    [DataField]
    public float AscensuionMultiplier = 3f;

    [DataField]
    public DamageSpecifier ToHeal = new()
    {
        DamageDict =
        {
            {"Blunt", -1},
            {"Slash", -1},
            {"Piercing", -1},
            {"Heat", -1},
            {"Cold", -1},
            {"Shock", -1},
            {"Asphyxiation", -1},
            {"Bloodloss", -1},
            {"Caustic", -1},
            {"Poison", -1},
            {"Radiation", -1},
            {"Cellular", -1},
            {"Holy", -1},
        },
    };

    [DataField]
    public float StaminaHeal = 10f;

    [DataField]
    public FixedPoint2 BloodHeal = 5f;

    [DataField]
    public TimeSpan StunReduction = TimeSpan.FromSeconds(1f);

    [DataField]
    public float TargetTemperature = 310f;
}
