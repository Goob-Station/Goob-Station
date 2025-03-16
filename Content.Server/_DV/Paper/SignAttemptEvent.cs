namespace Content.Server._DV.Paper;

/// <summary>
/// 	Raised on the pen when trying to sign a paper.
/// 	If it's cancelled the signature wasn't made.
/// </summary>
[ByRefEvent]
public record struct SignAttemptEvent(EntityUid Paper, EntityUid User, bool Cancelled = false);
