using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components.Mobs;

[RegisterComponent, NetworkedComponent]
public sealed partial class SummonRatDenComponent : Component
{
    [DataField]
    public EntProtoId RatDen = "RatDen";

    [DataField]
    public EntityUid? RatDenUid;
}
