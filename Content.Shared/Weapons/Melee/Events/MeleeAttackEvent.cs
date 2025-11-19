// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Weapons.Melee.Events;

/// <summary>
/// Event raised on the user after attacking with a melee weapon, regardless of whether it hit anything.
/// </summary>
[ByRefEvent]
public record struct MeleeAttackEvent(EntityUid Weapon);
