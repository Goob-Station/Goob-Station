namespace Content.Shared._Lavaland.Tile.Shapes;

/// <summary>
/// Represents a simple shape out of two diagonal lines
/// combined, similar to how Bishop chess piece moves.
/// </summary>
public sealed partial class BishopTileShape : TileShape
{
    [DataField]
    public int Range = 5;

    public override List<Vector2i> GetShape(Vector2i center)
    {
        return TileHelperMethods.MakeCrossDiagonal(center, Range);
    }
}
