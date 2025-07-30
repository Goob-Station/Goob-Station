using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Tile.Shapes;

/// <summary>
/// Represents a simple shape out of one horizontal and one vertical line
/// combined, similar to how Rook chess piece moves.
/// </summary>
public sealed partial class RookTileShape : TileShape
{
    protected override List<Vector2i> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        return TileHelperMethods.MakeCross(Offset, Size).ToList();
    }
}
