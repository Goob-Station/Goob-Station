// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Weapons.Melee.Components;
namespace Content.Shared.Weapons.Melee.Events;

/// <summary>
/// Raised directed on a weapon when attempt a melee attack.
/// </summary>
[ByRefEvent]
// Shitmed Change - Added Weapon and WeaponComponent
public record struct AttemptMeleeEvent(EntityUid User, EntityUid Weapon, MeleeWeaponComponent WeaponComponent, bool Cancelled = false, string? Message = null);
