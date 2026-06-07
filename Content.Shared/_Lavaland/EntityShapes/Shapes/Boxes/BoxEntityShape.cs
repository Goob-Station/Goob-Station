using System.Linq;
using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes.Shapes;

public sealed partial class BoxEntityShape : EntityShape
{
    [DataField]
    public bool Hollow;

    protected override List<Vector2> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        return ShapeHelpers.MakeBox(Offset, Size, Hollow, StepSize).ToList();
    }
}
