using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components.Mobs;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class RalliedComponent : Component
{
    /// <summary>
    /// Damage multiplier to rallied mob.
    /// </summary>
    [DataField]
    public float RalliedStrength = 1.5f;

    /// <summary>
    /// Attack speed multiplier to rallied mob.
    /// </summary>
    [DataField]
    public float RalliedAttackSpeed = 1.5f;

    /// <summary>
    /// The original attack damage, in order to reset it later
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier? OriginalDamage;

    /// <summary>
    /// The original attack speed, in order to reset it later
    /// </summary>
    [DataField, AutoNetworkedField]
    public float OriginalSpeed;
}
