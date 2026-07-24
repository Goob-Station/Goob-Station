// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.MartialArts;

[ByRefEvent]
public readonly record struct AfterComboCheckEvent(EntityUid Performer, EntityUid Target, EntityUid Weapon, ComboAttackType Type);
