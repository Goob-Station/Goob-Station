namespace Content.Goobstation.Common.MartialArts;

[ByRefEvent]
public readonly record struct AfterComboCheckEvent(EntityUid Performer, EntityUid Target, EntityUid Weapon, ComboAttackType Type);
