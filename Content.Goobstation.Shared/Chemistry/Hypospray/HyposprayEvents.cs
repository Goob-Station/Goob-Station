namespace Content.Goobstation.Shared.Chemistry.Hypospray;

/// <summary>
/// Raised on a hypospray when it successfully injects.
/// </summary>
[ByRefEvent]
public record struct AfterHyposprayInjectsEvent()
{
    /// <summary>
    /// Entity that used the hypospray.
    /// </summary>
    public EntityUid User;

    /// <summary>
    /// Entity that was injected.
    /// </summary>
    public EntityUid Target;
}
