using Content.Goobstation.Maths.FixedPoint;

namespace Content.Goobstation.Server.Lollypop;

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed partial class LollypopComponent : Component
{
    [DataField]
    public TimeSpan NextBite = TimeSpan.Zero;

    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(1f);

    [DataField]
    public FixedPoint2 Ammount = FixedPoint2.New(0.1);

    [DataField]
    public EntityUid? HeldBy = null;

    // temporary variables

    [DataField]
    public FixedPoint2? OldTransferAmount;

}
