namespace Content.Goobstation.Shared.OverlaysAnimation.Animations;

public abstract class OverlayAnimation
{
    [DataField]
    public string? Name;

    [DataField]
    public AnimationType AnimationType = AnimationType.Linear;

    /// <summary>
    /// How long we should wait until playing this animation.
    /// </summary>
    [DataField]
    public float StartDelay;

    /// <summary>
    /// How long it takes to play the animation.
    /// </summary>
    [DataField]
    public float Duration = 1.0f;

    /// <summary>
    /// If specified, will be passed into the exponent
    /// function to play animation faster or slower than normal.
    /// </summary>
    [DataField]
    public float? ExponentSpeed;
}
