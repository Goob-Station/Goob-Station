// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Targeting;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.Ranged.ProjectileForceTarget;

/// <summary>
/// Forces the projectile with this component always hit the said part (by setting the shooter's targeting to it :trollface:)
/// Can also be used to make projectiles never miss.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ProjectileForceTargetComponent : Component
{
    /// <summary>
    /// Null means target does not get changed.
    /// </summary>
    [DataField]
    public TargetBodyPart? Part;

    [DataField]
    public bool MakeUnableToMiss;
}
