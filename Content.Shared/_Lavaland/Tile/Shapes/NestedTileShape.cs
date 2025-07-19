using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Tile.Shapes;

/// <summary>
/// Shape that covers a ProtoId containing other shapes.
/// </summary>
public sealed partial class NestedTileShape : TileShape
{
    [DataField(required: true)]
    public ProtoId<TileShapePrototype> Id;

    protected override List<Vector2i> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        return proto.Index(Id).Shape.GetShape(rand, proto, Offset, Size);
    }
}
