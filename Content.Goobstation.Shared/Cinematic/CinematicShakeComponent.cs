
namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// It shakes a camera. 
/// </summary>
[RegisterComponent]
public sealed partial class CinematicShakeComponent : Component
{
    /// <summary>
    /// Jitter amplitude (tiles) at full strength.
    /// </summary>
    [DataField]
    public float Amplitude = 0.08f;

    [DataField]
    public float ZoomJitter = 0.03f;

    [DataField]
    public float KeyframeRate = 12f;

    /// <summary>
    /// How far away (tiles) other viewers still feel the shake.
    /// </summary>
    [DataField]
    public float Range = 14f;

    [DataField]
    public float Strength;
}
