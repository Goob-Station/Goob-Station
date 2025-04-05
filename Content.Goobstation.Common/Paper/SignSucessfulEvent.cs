namespace Content.Goobstation.Common.Paper;

/// <summary>
/// 	Raised on the paper when a sign is successful.
/// </summary>
[ByRefEvent]
public record struct SignSuccessfulEvent(EntityUid Paper, EntityUid User);
