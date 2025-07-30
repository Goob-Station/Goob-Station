using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Tile.Shapes;

/// <summary>
/// Represents a simple shape out of two diagonal lines
/// combined, similar to how Bishop chess piece moves.
/// </summary>
public sealed partial class BishopTileShape : TileShape
{
    protected override List<Vector2i> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        return TileHelperMethods.MakeCrossDiagonal(Offset, Size).ToList();
    }
}
