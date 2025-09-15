using Linguini.Shared.Util;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SummonVoidCreatureComponent : Component
{
    [DataField]
    public EntProtoId CommanderProto = "MobSkeletonGoon";

    [DataField]
    public EntProtoId FiendProto = "MobBaseSyndicateKobold";

    [DataField]
    public EntProtoId HoundProto = "MobPossum";
}
