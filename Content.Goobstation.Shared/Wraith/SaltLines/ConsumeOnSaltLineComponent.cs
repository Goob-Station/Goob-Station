using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.SaltLines;

[RegisterComponent, NetworkedComponent]
public sealed partial class ConsumeOnSaltLineComponent : Component
{
    /// <summary>
    /// Amount to consume on use
    /// </summary>
    [DataField]
    public FixedPoint2 Amount = 1f;
}

/// <summary>
/// Raised when attempting to place a salt line
/// </summary>
[ByRefEvent]
public record struct AttemptSaltLineEvent(EntityUid User,bool Cancelled = false);

