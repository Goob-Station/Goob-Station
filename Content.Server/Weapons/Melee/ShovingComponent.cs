namespace Content.Server.Weapons.Melee;

[RegisterComponent]
public sealed partial class ShovingComponent : Component
{
    /// <summary>
    ///     Default shoving stamina damage used if the shoving entity has no ShovingComponent. See <see cref="StaminaDamage"/>.
    /// </summary>
    public const float DefaultStaminaDamage = 10f; // WWDP shoving

    /// <summary>
    ///     Amount of stamina damage dealt on successful shove if the attacker has a 100% chance to shove the target.
    ///     If the chance is less than 100% (which it almost always is), the damage is multiplied by the chance.
    /// </summary>
    [DataField]
    public float StaminaDamage = DefaultStaminaDamage;

    /// <summary>
    ///     Added to the shove/disarm chance on attacks done by this mob, acts opposite to DisarmMalus for targets.
    /// </summary>
    [DataField]
    public float DisarmBonus = 0f;
}