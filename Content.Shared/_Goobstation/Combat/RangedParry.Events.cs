using Content.Shared.Projectiles;

namespace Content.Shared._Goobstation.Combat;

[ByRefEvent]
public record struct ProjectileParryAttemptEvent(EntityUid ProjUid, EntityUid Target, ProjectileComponent Component, bool Cancelled);
