using System.Numerics;

namespace Content.Server._Mono.Radar;

/// <summary>
/// Stores the trajectory information for hitscan projectiles to be visualized on radar.
/// Tracks the start and end points of the hitscan beam to draw a line on the scanner.
/// </summary>
[RegisterComponent]
public sealed partial class HitscanRadarComponent : Component
{
    /// <summary>
    /// Color that gets shown on the radar screen for the hitscan line.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("radarColor")]
    public Color RadarColor = Color.Magenta;

    /// <summary>
    /// Start position of the hitscan beam in world coordinates.
    /// </summary>
    [DataField]
    public Vector2 StartPosition;

    /// <summary>
    /// End position of the hitscan beam in world coordinates.
    /// </summary>
    [DataField]
    public Vector2 EndPosition;

    /// <summary>
    /// Thickness of the line drawn on the radar.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("lineThickness")]
    public float LineThickness = 1.0f;

    /// <summary>
    /// The grid that the hitscan is associated with, if any.
    /// This is used for coordinate transformations when drawing on the radar.
    /// </summary>
    [DataField]
    public EntityUid? OriginGrid;

    /// <summary>
    /// Controls whether this hitscan line is visible on radar.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled")]
    public bool Enabled = true;

    /// <summary>
    /// Time this hitscan radar blip should remain visible before being automatically removed.
    /// </summary>
    [DataField]
    public float LifeTime = 0.5f;
}
