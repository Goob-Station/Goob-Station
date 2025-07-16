using System.Linq;

namespace Content.Shared._Lavaland.Tile.Shapes;

/// <summary>
/// Represents a more complex shape out of Bishop and Rook shapes
/// combined, similar to how Queen chess piece moves.
/// </summary>
public sealed partial class QueenTileShape : TileShape
{
    [DataField]
    public int Range = 5;

    public override List<Vector2i> GetShape(Vector2i center)
    {
        var cross = TileHelperMethods.MakeCross(center, Range).ToHashSet();
        var diagcross = TileHelperMethods.MakeCrossDiagonal(center, Range).ToHashSet();
        var both = cross.Concat(diagcross).ToHashSet().ToList();
        return both;
    }
}
