namespace Content.Goobstation.Shared.Damage.Components;

/// <summary>
/// Makes target hit puke.
/// </summary>
[RegisterComponent]
public sealed partial class VomitOnHitComponent : Component
{
    /// <summary>
    /// Chance (0-1) for each hit mob to vomit.
    /// </summary>
    [DataField]
    public float Chance = 0.5f;

    /// <summary>
    /// How much thirst the vomit adds (negative drains it).
    /// </summary>
    [DataField]
    public float ThirstAdded = -40f;

    /// <summary>
    /// How much hunger the vomit adds (negative drains it).
    /// </summary>
    [DataField]
    public float HungerAdded = -40f;

    /// <summary>
    /// Whether the vomit ignores the usual checks (e.g. already being unable to vomit).
    /// </summary>
    [DataField]
    public bool Force;
}
