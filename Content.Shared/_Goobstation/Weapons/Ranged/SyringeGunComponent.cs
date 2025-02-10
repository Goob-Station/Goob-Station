namespace Content.Shared._Goobstation.Weapons.Ranged;

/// <summary>
///     Component that allows guns to instantly inject all the contents of any syringe on a target.
/// </summary>
[RegisterComponent]
public sealed partial class SyringeGunComponent : Component
{
    [DataField]
    public bool PierceArmor;

    /// <summary>
    ///     Multiplies injection speed for fired syringes with SolutionInjectWhileEmbeddedComponent.
    /// </summary>
    [DataField]
    public float InjectionSpeedMultiplier = 1f;
}
