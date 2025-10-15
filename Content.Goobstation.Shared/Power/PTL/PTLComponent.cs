// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <mcbosserson@hotmail.com>
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

    [DataField] public double MinShootPower = 1e6f; // 1 MJ
    [DataField] public double MaxEnergyPerShot = 5e6; // 5 MJ

    [DataField, AutoNetworkedField] public float ShootDelay = 10f;
    [DataField, AutoNetworkedField] public float ShootDelayIncrement = 5f;
    [DataField, AutoNetworkedField] public MinMax ShootDelayThreshold = new MinMax(10, 60);
    [DataField, AutoNetworkedField] public bool ReversedFiring = false;
    [ViewVariables(VVAccess.ReadOnly)] public TimeSpan NextShotAt = TimeSpan.Zero;
    [ViewVariables(VVAccess.ReadOnly)] public TimeSpan RadDecayTimer = TimeSpan.Zero;

    [DataField] public DamageSpecifier BaseBeamDamage;

    /// <summary>
    ///     The factor that power (in MJ) is multiplied by to calculate radiation and blinding.
    /// </summary>
    [DataField] public double EvilMultiplier = 0.1;
}
