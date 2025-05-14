namespace Content.Goobstation.Shared.OverlaysAnimation;

public enum AnimationType
{
    /// <summary>
    /// Do this animation instantly when it should start.
    /// After that will wait until the next animation
    /// </summary>
    Instant,

    /// <summary>
    ///
    /// </summary>
    Linear,

    /// <summary>
    /// Slow at the start, and accelerates on the end.
    /// </summary>
    Exponential,

    /// <summary>
    /// Faster in the middle, slower at the start and the end.
    /// </summary>
    Sinus,

    /// <summary>
    /// Faster at the start and the end, slower in the middle.
    /// </summary>
    Cosinus,
}
