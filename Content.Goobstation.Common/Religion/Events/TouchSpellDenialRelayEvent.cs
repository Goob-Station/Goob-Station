namespace Content.Goobstation.Common.Religion.Events;

/// <summary>
/// Event broadcast when a touch spell is cancelled.
/// </summary>
[ByRefEvent]
public record struct TouchSpellDenialRelayEvent(bool Cancelled = false);
