namespace Content.Shared._Trauma.Projectiles;

/// <summary>
/// Event broadcast when a projectile is shot with a non-null user.
/// </summary>
[ByRefEvent]
public record struct PlayerShotProjectileEvent(EntityUid Projectile, EntityUid User);
