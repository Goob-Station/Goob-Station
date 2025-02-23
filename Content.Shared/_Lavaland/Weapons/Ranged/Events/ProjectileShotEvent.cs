namespace Content.Shared._Lavaland.Weapons.Ranged.Events;

/// <summary>
/// Raised on a gun when a projectile has been fired from it.
/// </summary>
public sealed class ProjectileShotEvent : EntityEventArgs
{
    public EntityUid FiredProjectile = default!;
}


