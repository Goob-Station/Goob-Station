// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Weapons.DelayedKnockdown;

[RegisterComponent]
public sealed partial class DelayedKnockdownOnHitComponent : Component
{
    [DataField]
    public float Delay = 2f;

    [DataField]
    public float KnockdownTime = 4f;

    [DataField]
    public bool Refresh = true;

    [DataField]
    public bool ApplyOnHeavyAttack;

    [DataField]
    public string UseDelay = "default";
}