namespace Content.Goobstation.Common.Paper;

/// <summary>
/// 	Raised on the paper when a sign is attempted
/// </summary>
[ByRefEvent]
public record struct BeingSignedAttemptEvent(EntityUid Paper, EntityUid Signer, bool Cancelled = false);
