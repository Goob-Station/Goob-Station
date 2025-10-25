using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.SaltLines;

[RegisterComponent, NetworkedComponent]
public sealed partial class ConsumeOnSaltLineComponent : Component
{
    [DataField]
    public FixedPoint2 Amount = 1f;
}

[ByRefEvent]
public record struct AttemptSaltLineEvent(bool Cancelled = false);
