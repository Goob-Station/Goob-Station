using System.Linq;
using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes.Shapes;

/// <summary>
/// Represents a simple shape out of two diagonal lines
/// combined, similar to how Bishop chess piece moves.
/// </summary>
public sealed partial class BishopEntityShape : EntityShape
{
    protected override List<Vector2> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        return ShapeHelpers.MakeCrossDiagonal(Offset, Size, StepSize).ToList();
    }
}
