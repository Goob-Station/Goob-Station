namespace Content.Goobstation.Shared.HeatTint;

[RegisterComponent]
public sealed partial class HeatTintComponent : Component
{
    /// <summary>
    /// Color gradient stops sorted by temperature (Kelvin).
    /// The system interpolates between adjacent stops in OkLab color space.
    /// Must have at least 2 entries.
    /// </summary>
    [DataField(required: true)]
    public List<HeatTintPoint> ColorGradient = new();

    /// <summary>
    /// Optional sprite layer map keys to tint.
    /// If null or empty, tints the entire sprite.
    /// </summary>
    [DataField]
    public List<string>? AffectedLayers;

    public Dictionary<int, Color> BaseColors = new();

    public Dictionary<int, Color> LastAppliedColors = new();
}

[DataDefinition]
public sealed partial class HeatTintPoint
{
    [DataField(required: true)]
    public float Temperature;

    [DataField(required: true)]
    public Color Color = Color.White;
}
