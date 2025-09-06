using Content.Goobstation.Maths.FixedPoint;

namespace Content.Goobstation.Server.Lollypop;

[RegisterComponent]
public sealed partial class LollypopComponent : Component
{
    [DataField]
    public FixedPoint2 Ammount = FixedPoint2.New(0.1);

    [DataField]
    public EntityUid? HeldBy = null;

}
