namespace Content.Goobstation.Shared.StationRadio.Events;

/// <summary>
/// Raised on a vinyl player when a vinyl is inserted and starts playing.
/// </summary>
[ByRefEvent]
public record struct VinylInsertedEvent(EntityUid Vinyl);

/// <summary>
/// Raised on a vinyl player when a vinyl is removed.
/// </summary>
[ByRefEvent]
public record struct VinylRemovedEvent(EntityUid Vinyl);
