namespace Content.Goobstation.Common.Weapons.Ranged;

/// <summary>
/// Raised on a user when projectiles have been fired from gun.
/// </summary>
public sealed class AmmoShotUserEvent : EntityEventArgs
{
    public EntityUid Gun;
    public List<EntityUid> FiredProjectiles = default!;
}
