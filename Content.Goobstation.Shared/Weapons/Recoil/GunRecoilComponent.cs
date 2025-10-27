// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Standing;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.Recoil;

[RegisterComponent, NetworkedComponent]
public sealed partial class GunRecoilComponent : Component
{
    [DataField]
    public float BaseThrowRange = 1f;

    [DataField]
    public float BaseThrowSpeed = 4f;

    [DataField]
    public bool AffectedByMass = true;

    [DataField]
    public float MassMultiplier = 70f;

    [DataField]
    public float BaseKnockdownTime = 1f;

    [DataField]
    public bool RefreshKnockdown = true;

    [DataField]
    public DropHeldItemsBehavior Behavior = DropHeldItemsBehavior.NoDrop;
}
