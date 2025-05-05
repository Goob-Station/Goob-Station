using Content.Shared.Damage;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Weapons.Ranged.ProjectileDamageWhitelist;

/// <summary>
/// If a projectile with this component collides with an entity that meets the whitelist, applies the damage.
/// </summary>
[RegisterComponent]
public sealed partial class ProjectileDamageWhitelistComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new ();

    [DataField]
    public bool IgnoreResistances;

    [DataField]
    public EntityWhitelist Whitelist = new ();
}
