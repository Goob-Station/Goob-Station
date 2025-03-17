using Content.Shared.Whitelist;

namespace Content.Shared._Goobstation.Weapons.Ranged;

/// <summary>
/// Allows a projectile to only hit entities on a whitelist.
/// </summary>
[RegisterComponent]
public sealed partial class ProjectileRequireWhitelistComponent : Component
{
    /// <summary>
    /// The whitelist for what the projectile can affect.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Whether the whitelist should be inverted.
    /// </summary>
    /// <remarks>
    /// If this is true, and the whitelist is set to clumsy, the projectile will affect anyone that does *not* have the clumsy component.
    /// </remarks>
    [DataField]
    public bool Invert = false;
}
