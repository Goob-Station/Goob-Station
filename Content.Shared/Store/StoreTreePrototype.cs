using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Shared.Store;

[Prototype]
public sealed partial class StoreTreePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name;

    [DataField]
    public Vector2 Home;

    [DataField(required: true)]
    public Dictionary<ProtoId<ListingPrototype>, StoreTreeNode> Nodes = new();

    [DataField]
    public List<StoreTreeLabel> Labels = new();
}

[DataDefinition]
public sealed partial class StoreTreeNode
{
    [DataField(required: true)]
    public Vector2 Position;

    [DataField]
    public List<ProtoId<ListingPrototype>> Parents = new();
}

[DataDefinition]
public sealed partial class StoreTreeLabel
{
    [DataField(required: true)]
    public LocId Text;

    [DataField(required: true)]
    public Vector2 Position;

    [DataField]
    public Color Color = Color.White;
}
