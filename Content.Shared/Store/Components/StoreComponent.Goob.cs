using Robust.Shared.Prototypes;

namespace Content.Shared.Store.Components;

public sealed partial class StoreComponent
{
    [DataField]
    public ProtoId<StoreTreePrototype>? Tree;
}
