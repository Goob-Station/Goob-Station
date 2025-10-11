using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Mobs;

/// <summary>
/// This is used for changing mob therholds values, and stores old values to revet them back
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ChangeThresholdComponent : Component
{
    [DataField]
    public FixedPoint2 OldCriticalThreshold = FixedPoint2.New(100);

    [DataField]
    public FixedPoint2 NewCriticalThreshold = FixedPoint2.New(150);

    [DataField]
    public FixedPoint2 OldDeadThreshold  = FixedPoint2.New(200);

    [DataField]
    public FixedPoint2 NewDeadThreshold = FixedPoint2.New(300);
}
