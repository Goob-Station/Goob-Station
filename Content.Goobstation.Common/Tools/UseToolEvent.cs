namespace Content.Goobstation.Common.Tools;

/// <summary>
/// Event raised on a tool in `SharedToolSystem.UseTool()` if its DoAfter timer successfully started.
/// </summary>
public readonly record struct UseToolEvent
{
    /// <summary>
    /// The entity using the tool.
    /// </summary>
    public readonly EntityUid User;

    /// <summary>
    /// The entity that the tool is being used on. (May be null)
    /// </summary>
    public readonly EntityUid? Target;

    /// <summary>
    /// The ID index of the DoAfter.
    /// </summary>
    /// <remarks>
    /// Ideally this would just be a <c>DoAfterIdx</c> instance and wouldn't need converting back, but this is in '.Common' so oh well.
    /// </remarks>
    public readonly ushort DoAfterIdx;

    /// <summary>
    /// Duration of the DoAfter timer.
    /// </summary>
    public readonly TimeSpan DoAfterLength;

    public UseToolEvent(EntityUid user, EntityUid? target, ushort doAfterIdx, TimeSpan doAfterLength)
    {
        User = user;
        Target = target;
        DoAfterIdx = doAfterIdx;
        DoAfterLength = doAfterLength;
    }
}
