using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Tile.Shapes;

public sealed partial class BoxTileShape : TileShape
{
    [DataField]
    public bool Hollow;

    protected override List<Vector2i> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        return TileHelperMethods.MakeBox(Offset, Size, Hollow);
    }
}
