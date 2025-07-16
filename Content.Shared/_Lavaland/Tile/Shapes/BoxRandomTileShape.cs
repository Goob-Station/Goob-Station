using Robust.Shared.Random;

namespace Content.Shared._Lavaland.Tile.Shapes;

/// <summary>
/// Creates a filled box, but also with a chance of a tile to be missing, making it have random cavities.
/// </summary>
public sealed partial class BoxRandomTileShape : TileShape
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [DataField]
    public int Dimensions = 5;

    [DataField]
    public float FilledChance = 0.3f;

    public override List<Vector2i> GetShape(Vector2i center)
    {
        return TileHelperMethods.MakeBoxRandom(center, Dimensions, _random, FilledChance);
    }
}
