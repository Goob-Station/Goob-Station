// SPDX-FileCopyrightText: 2024 Ashley Woodiss-Field <ash@DESKTOP-H64M4AI.localdomain>
// SPDX-FileCopyrightText: 2024 ColesMagnum <98577947+AW-FulCode@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;

namespace Content.Shared.Weapons.Ranged.Events;

/// <summary>
///     Raised directed on the gun entity when <see cref="SharedGunSystem.RefreshModifiers"/>
///     is called, to update the values of <see cref="GunComponent"/> from other systems.
/// </summary>
[ByRefEvent]
public record struct GunRefreshModifiersEvent(
    Entity<GunComponent> Gun,
    SoundSpecifier? SoundGunshot,
    float CameraRecoilScalar,
    Angle AngleIncrease,
    Angle AngleDecay,
    Angle MaxAngle,
    Angle MinAngle,
    int ShotsPerBurst,
    float FireRate,
    float ProjectileSpeed,
    float BurstFireRate, // Goobstation
    float BurstCooldown, // Goobstation
    EntityUid? User // GoobStation change - User for NoWieldNeeded
);