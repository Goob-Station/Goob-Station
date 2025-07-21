using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Tile.Shapes;

/// <summary>
/// Represents a simple line with length of Size
/// made in some specified direction.
/// </summary>
public sealed partial class LineTileShape : TileShape
{
    [DataField]
    public Vector2i Direction = Vector2i.Up;

    protected override List<Vector2i> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        return TileHelperMethods.MakeLine(Offset, Size, Direction);
    }
}
