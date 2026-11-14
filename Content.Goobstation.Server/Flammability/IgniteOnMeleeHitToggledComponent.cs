namespace Content.Goobstation.Server.Flammability;

/// <summary>
/// Adds fire stacks to entities hit with this item, but only while activated
/// </summary>
[RegisterComponent]
public sealed partial class IgniteOnMeleeHitToggledComponent : Component
{
    /// <summary>
    /// Fire stacks added to each entity hit
    /// </summary>
    [DataField]
    public float FireStacks;
}
