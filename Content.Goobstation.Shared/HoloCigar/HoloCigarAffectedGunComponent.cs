// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.HoloCigar;

/// <summary>
/// This is used for tracking affected HoloCigar weapons.
/// </summary>
[RegisterComponent]
public sealed partial class HoloCigarAffectedGunComponent : Component
{
    [ViewVariables]
    public EntityUid GunOwner = EntityUid.Invalid;

    [ViewVariables]
    public bool WasOriginallyMultishot = false;

    [ViewVariables]
    public float OriginalMissChance;

    [ViewVariables]
    public float OriginalSpreadModifier;

    [ViewVariables]
    public float OriginalSpreadAddition;

    [ViewVariables]
    public float OriginalHandDamage;

    [ViewVariables]
    public float OriginalStaminaDamage;
}
