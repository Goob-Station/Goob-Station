// SPDX-FileCopyrightText: 2024 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Weapons.Ranged.Events;

/// <summary>
///     Raised directed on the gun entity when ammo is shot to calculate its spread.
/// </summary>
/// <param name="Spread">The spread of the ammo, can be changed by handlers.</param>
[ByRefEvent]
public record struct GunGetAmmoSpreadEvent(Angle Spread);