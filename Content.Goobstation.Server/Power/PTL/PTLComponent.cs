// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Goobstation.Server.Power.PTL;

[RegisterComponent]
public sealed partial class PTLComponent : Component
{
    [DataField] public bool Active = false;

    [DataField] public double SpesosHeld = 0f;

    [DataField] public double MinShootPower = 1e6; // 1 MJ
    [DataField] public double MaxEnergyPerShot = 1e9; // 1 GJ

    [DataField] public TimeSpan ShootDelay = TimeSpan.FromSeconds(10);
    [ViewVariables(VVAccess.ReadOnly)] public TimeSpan NextShotAt = TimeSpan.Zero;

    [DataField] public DamageSpecifier BaseBeamDamage;

    /// <summary>
    ///     Amount of power required to start emitting radiation and blinding people that come nearby
    /// </summary>
    [DataField] public double PowerEvilThreshold = (50 * 1e6); // 50 MJ;
}
