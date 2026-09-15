namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// It zooms a camera.
/// </summary>
[RegisterComponent]
public sealed partial class CinematicZoomComponent : Component
{
    /// <summary>
    /// Zoom multiplier at full strength. Below 1 zooms in.
    /// </summary>
    [DataField]
    public float ZoomFactor = 0.75f;

    /// <summary>
    /// How far away (tiles) other viewers still feel the zoom.
    /// </summary>
    [DataField]
    public float Range = 14f;

    [DataField]
    public float Strength;
}
