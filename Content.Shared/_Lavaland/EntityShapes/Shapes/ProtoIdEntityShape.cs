using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes.Shapes;

/// <summary>
/// Works like NestedEntityShape, but also has serialization,
/// so you can just type id: in prototypes and it will work.
/// </summary>
public sealed partial class ProtoIdEntityShape : EntityShape
{
    public const string IdDataFieldTag = "id";

    [DataField(IdDataFieldTag, required: true)]
    public ProtoId<EntityShapePrototype> Id;

    protected override List<Vector2> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        return proto.Index(Id).Shape.GetShape(rand, proto, Offset, Size, StepSize);
    }
}
