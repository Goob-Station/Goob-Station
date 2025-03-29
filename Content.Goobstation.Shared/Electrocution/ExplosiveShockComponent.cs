using Content.Shared.Damage;

namespace Content.Goobstation.Shared.Electrocution;

[RegisterComponent]
public sealed partial class ExplosiveShockComponent : Component
{
    /// <summary>
    ///     Additional damage to deal to all hands on top of the explosion damage.
    /// </summary>
    [DataField]
    public DamageSpecifier HandsDamage = default!;

    /// <summary>
    ///     Additional damage to deal to all arms on top of the explosion damage.
    /// </summary>
    [DataField]
    public DamageSpecifier ArmsDamage = default!;

    /// <summary>
    ///     How many seconds to knockdown the wearer for after the explosion.
    /// </summary>
    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(2);

    /// <summary>
    ///     How many seconds to wait after the shock before the explosion.
    /// </summary>
    [DataField]
    public TimeSpan ExplosionDelay = TimeSpan.FromSeconds(1);
}
