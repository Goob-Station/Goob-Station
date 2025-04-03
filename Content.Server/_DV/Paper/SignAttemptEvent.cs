<<<<<<< HEAD
namespace Content.Shared._DV.Paper;

/// <summary>
/// Raised on the pen when trying to sign a paper.
/// If it's cancelled the signature isn't made.
=======
namespace Content.Server._DV.Paper;

/// <summary>
/// 	Raised on the pen when trying to sign a paper.
/// 	If it's cancelled the signature wasn't made.
>>>>>>> 38c6f918769a01f7fc55d94840f7b239992b85fe
/// </summary>
[ByRefEvent]
public record struct SignAttemptEvent(EntityUid Paper, EntityUid User, bool Cancelled = false);
