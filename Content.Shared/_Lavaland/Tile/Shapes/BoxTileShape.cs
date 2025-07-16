namespace Content.Shared._Lavaland.Tile.Shapes;

public sealed partial class BoxTileShape : TileShape
{
    [DataField]
    public int Dimensions = 5;

    [DataField]
    public bool Filled = true;

    public override List<Vector2i> GetShape(Vector2i center)
    {
        return TileHelperMethods.MakeBox(center, Dimensions, Filled);
    }
}
