namespace Content.Goobstation.Common.Cloning;

/// <summary>
/// Raised on the original body when its clone has a mind added (usually via the cloning EUI)
/// </summary>
[ByRefEvent]
public record struct TransferredToCloneEvent(EntityUid Cloned);
