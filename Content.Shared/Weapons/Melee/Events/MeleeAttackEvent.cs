// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Weapons.Melee.Events;

/// <summary>
/// Event raised on the user after attacking with a melee weapon, regardless of whether it hit anything.
/// </summary>
[ByRefEvent]
public record struct MeleeAttackEvent(EntityUid Weapon);