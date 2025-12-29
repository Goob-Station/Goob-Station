using System.Linq;
using System.Numerics;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes.Shapes;

/// <summary>
/// Picks one shape out of a list of children using weights to randomize between them.
/// </summary>
public sealed partial class GroupEntityShape : EntityShape
{
    [DataField(required: true)]
    public List<EntityShape> Children = new();

    protected override List<Vector2> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        var children = new Dictionary<EntityShape, float>(Children.Count);
        foreach (var child in Children)
        {
            children.Add(child, child.Weight);
        }

        if (children.Count == 0)
            return Enumerable.Empty<Vector2>().ToList();

        var pick = SharedRandomExtensions.Pick(children, rand);
        return pick.GetShape(rand, proto, Offset, Size, StepSize);
    }
}
