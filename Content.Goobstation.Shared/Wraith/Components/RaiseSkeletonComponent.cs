using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class RaiseSkeletonComponent : Component
{
    [DataField]
    public EntProtoId SkeletonProto = "MobSkeletonGoon";
}
