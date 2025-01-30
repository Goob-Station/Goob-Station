using Content.Shared.Damage;

namespace Content.Server.Electrocution;

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
    ///     How many seconds to wait after the shock before the explosion.
    /// </summary>
    [DataField]
    public float SizzleTime = 1f;

    /// <summary>
    ///     How many seconds to knockdown the wearer for after the explosion.
    /// </summary>
    [DataField]
    public float KnockdownTime = 2f;
}
