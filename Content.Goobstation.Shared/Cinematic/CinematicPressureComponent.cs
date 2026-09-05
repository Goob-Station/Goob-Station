namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// Aura but it's on your screen bro.
/// </summary>
[RegisterComponent]
public sealed partial class CinematicPressureComponent : Component
{
    [DataField]
    public string Shader = "HereticPressure";

    [DataField]
    public Color Color = Color.FromHex("#a755ff");

    [DataField]
    public float FlowSpeed = 1f;

    [DataField]
    public float Intensity = 1f;

    [DataField]
    public float FadeInTime = 1.1f;

    [DataField]
    public float FadeOutTime = 1.8f;

    #region Shockwave

    [DataField]
    public float ShockDuration;

    /// <summary>
    /// Seconds after the aura starts that the wave is thrown.
    /// </summary>
    [DataField]
    public float ShockTime;

    #endregion

    [DataField]
    public float Strength;

    [DataField]
    public float Current;

    [DataField]
    public float Age;

    [DataField]
    public float Shock = -1f;
}
