namespace Content.Shared._Lavaland.Tile.Shapes;

/// <summary>
/// Represents a simple shape out of one horizontal and one vertical line
/// combined, similar to how Rook chess piece moves.
/// </summary>
public sealed partial class RookTileShape : TileShape
{
    [DataField]
    public int Range = 5;

    public override List<Vector2i> GetShape(Vector2i center)
    {
        return TileHelperMethods.MakeCross(center, Range);
    }
}
