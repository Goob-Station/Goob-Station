namespace Content.Goobstation.Common.Weapons;

[ByRefEvent]
public record struct HeavyAttackTargetAttemptEvent(EntityUid User, EntityUid Weapon, bool Cancelled = false);
