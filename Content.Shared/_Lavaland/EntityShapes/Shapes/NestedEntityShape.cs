using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes.Shapes;

/// <summary>
/// Shape that references a ProtoId containing some other shape.
/// </summary>
public sealed partial class NestedEntityShape : EntityShape
{
    [DataField(required: true)]
    public ProtoId<EntityShapePrototype> Id;

    protected override List<Vector2> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        return proto.Index(Id).Shape.GetShape(rand, proto, Offset, Size, StepSize);
    }
}
