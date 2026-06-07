using System.Linq;
using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes.Shapes;

/// <summary>
/// Represents a simple shape out of one horizontal and one vertical line
/// combined, similar to how Rook chess piece moves.
/// </summary>
public sealed partial class RookEntityShape : EntityShape
{
    protected override List<Vector2> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        return ShapeHelpers.MakeCross(Offset, Size, StepSize).ToList();
    }
}
