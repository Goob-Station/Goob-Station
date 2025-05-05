namespace Content.Goobstation.Common.DoAfter;

/// <summary>
/// Event raised on the doafter user after a doafter ends.
/// </summary>
[ByRefEvent]
public readonly record struct DoAfterEndedEvent(EntityUid? Target, bool Cancelled);
