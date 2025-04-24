// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Power.PTL;

[RegisterComponent]
public sealed partial class PTLComponent : Component
{
    [DataField] public bool Active = false;

    [DataField] public float SpesosPerJoule = 0.001f;
    [DataField] public float MinShootPower = 1000000; // 1 MJ
    [DataField] public float MaxEnergyPerShot = 1000000000f; // 1 GJ
    [DataField] public TimeSpan ShootDelay = TimeSpan.FromSeconds(10);
    public TimeSpan NextShotAt = TimeSpan.Zero; // yea just hold this

    [DataField] public float SpesosHeld = 0f;
}
