using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SummonRatDenComponent : Component
{
    [DataField]
    public EntProtoId RatDenProto = "RatDen";
}
