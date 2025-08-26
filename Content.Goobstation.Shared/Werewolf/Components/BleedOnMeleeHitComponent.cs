namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent]
public sealed partial class BleedOnMeleeHitComponent : Component
{
    /// <summary>
    ///  The amount of bleeding to apply to the target
    /// </summary>
    [DataField]
    public float BleedAmount;
}
