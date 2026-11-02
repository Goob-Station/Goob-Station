namespace Content.Goobstation.Shared.Damage.Components;

/// <summary>
/// Makes target hit puke.
/// </summary>
[RegisterComponent]
public sealed partial class VomitOnHitComponent : Component
{
    /// <summary>
    /// Chance to cause puking.
    /// </summary>
    [DataField]
    public float Chance = 0.5f;

    /// <summary>
    /// Make these positive if you don't want this to be super OP
    /// Drains thrist if negative, adds if positive.
    /// </summary>
    [DataField]
    public float? ThirstAdded;

    /// <summary>
    /// Make these positive if you don't want this to be super OP
    /// Drains hunger if negative, adds if positive.
    /// </summary>
    [DataField]
    public float? HungerAdded;

    /// <summary>
    /// Force puke even if can't usually puke.
    /// </summary>
    [DataField]
    public bool Force;
}
