namespace Content.Shared.Weapons.Ranged.Components;
public sealed partial class GunComponent : Component
{
    /// <summary>
    /// [Goobstation]
    /// How far in front of the shooter, in tiles, projectiles and hitscan shots start.
    /// Moves the start point from the shooter's center to the gun's muzzle so walls behind the shooter aren't hit.
    /// </summary>
    [DataField]
    public float MuzzleDistance = 0.2f;
}
