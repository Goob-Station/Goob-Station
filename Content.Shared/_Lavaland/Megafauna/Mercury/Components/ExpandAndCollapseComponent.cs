
namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Expands the sprite of an entity, then collapses it once it reaches X size. Best paired with a TimedDespawn or something of the sort, or an explosion.
/// </summary>

[RegisterComponent]
public sealed partial class ExpandAndCollapseComponent : Component
{
    /// <summary>
    /// How long to expand the sprite for.
    /// </summary>
    [DataField]
    public float ExpandTime = 9f;

    /// <summary>
    /// How long the collapse lasts.
    /// </summary>
    [DataField]
    public float CollapseTime = 1f;

    /// <summary>
    /// The max scale to reach before collapsing.
    /// </summary>
    [DataField]
    public float StartingScale = 0.1f;

    public float CurrentScale;

    /// <summary>
    /// The max scale to reach before collapsing.
    /// </summary>
    [DataField]
    public float MaxScale = 1f;

    public float Accumulator;
    public bool Collapsing;
}
