using Content.Goobstation.Maths.FixedPoint;

namespace Content.Server.Body.Events;

// Pirate I hope its related enough to the files around so I dont have to move it to Pirate
/// <summary>
///     Raised on a mob when its bloodstream tries to perform natural blood regeneration.
/// </summary>
[ByRefEvent]
public sealed class NaturalBloodRegenerationAttemptEvent : CancellableEntityEventArgs
{
    /// <summary>
    ///     How much blood the mob will regenerate on this tick. Can be negative.
    /// </summary>
    public FixedPoint2 Amount;
}
