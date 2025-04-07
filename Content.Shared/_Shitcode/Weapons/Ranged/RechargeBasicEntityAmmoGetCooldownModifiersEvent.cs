// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared._Goobstation.Weapons.Ranged;

// todo: get event names closer to the length of the bible
[ByRefEvent]
public record struct RechargeBasicEntityAmmoGetCooldownModifiersEvent(
    float Multiplier
);