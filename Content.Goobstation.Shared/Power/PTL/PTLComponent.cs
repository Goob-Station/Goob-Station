// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SoundingExpert <204983230+SoundingExpert@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 john git <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Damage;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Power.PTL;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PTLComponent : Component
{
    [DataField, AutoNetworkedField] public bool Active = false;

    [DataField, AutoNetworkedField] public double SpesosHeld = 0f;

    [DataField] public double MinShootPower = 1e6; // 1 MJ 
    [DataField] public double MaxEnergyPerShot = 1e7; // 100MJ so powernet isnt nuked

    [DataField, AutoNetworkedField] public float ShootDelay = 15f; //So Laser can build a charge
    [DataField, AutoNetworkedField] public MinMax ShootDelayThreshold = new MinMax(15, 60);
    [DataField, AutoNetworkedField] public bool ReversedFiring = false;
    [ViewVariables(VVAccess.ReadOnly)] public TimeSpan NextShotAt = TimeSpan.Zero;

    [DataField] public DamageSpecifier BaseBeamDamage;

    /// <summary>
    ///     Amount of power required to start emitting radiation and blinding people that come nearby
    /// </summary>
    [DataField] public double PowerEvilThreshold = 50e6; // 50 MJ Chudstation had this at 50J thats why it kept irradiating people
}
