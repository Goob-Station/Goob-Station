namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// Cinematic black bars that appear on the top / bottom of your screen.
/// </summary>
[RegisterComponent]
public sealed partial class CinematicLetterboxComponent : Component
{
    /// <summary>
    /// Height of each letterbox bar as a fraction of the viewport.
    /// </summary>
    [DataField]
    public float Fraction = 0.11f;

    /// <summary>
    /// How soft the edges are.
    /// </summary>
    [DataField]
    public float Feather = 0.22f;

    /// <summary>
    /// Color of the bars.
    /// </summary>
    [DataField]
    public Color Color = Color.Black;

    [DataField]
    public float Strength;
}
