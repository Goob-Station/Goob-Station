using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SummonLesserSkeletonComponent : Component
{
    [DataField]
    public EntProtoId SkeletonProto = "MobSkeletonGoonLesser";
}
