using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Tile.Shapes;

/// <summary>
/// Works like NestedTileShape, but also has serialization,
/// so you can just type id: in prototypes and it will work.
/// </summary>
public sealed partial class ProtoIdTileShape : TileShape
{
    public const string IdDataFieldTag = "id";

    [DataField(IdDataFieldTag, required: true)]
    public ProtoId<TileShapePrototype> Id;

    protected override List<Vector2i> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        return proto.Index(Id).Shape.GetShape(rand, proto, Offset, Size);
    }
}
