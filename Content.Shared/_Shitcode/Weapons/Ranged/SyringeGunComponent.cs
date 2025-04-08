namespace Content.Shared._Goobstation.Weapons.Ranged;

/// <summary>
///     Component that allows syringe-firing guns to uncap their injection limit on firing.
/// </summary>
[RegisterComponent]
public sealed partial class SyringeGunComponent : Component
{
    /// <summary>
    ///     Force fired projectiles to (not) pierce armor.
    ///     Doesn't apply if null.
    /// </summary>
    [DataField]
    public bool? PierceArmor;

    /// <summary>
    ///     Multiplies injection speed for fired syringes with SolutionInjectWhileEmbeddedComponent.
    /// </summary>
    [DataField]
    public float InjectionSpeedMultiplier = 1f;
}
