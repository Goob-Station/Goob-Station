using System.Linq;
using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes.Shapes;

/// <summary>
/// Tile shape that is made of multiple other shapes.
/// </summary>
public sealed partial class AllEntityShape : EntityShape
{
    [DataField(required: true)]
    public List<EntityShape> Children = new();

    [DataField]
    public Dictionary<string, GroupEntityShapeOptions>? Options;

    protected override List<Vector2> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        var result = new List<Vector2>();
        foreach (var child in Children)
        {
            Vector2? offset = Offset;
            int? size = Size;
            float? stepSize = StepSize;

            if (Options != null
                && child.OverrideGroup != null
                && Options.TryGetValue(child.OverrideGroup, out var options))
            {
                offset = options.Offset;
                size = options.GroupSize;
                stepSize = options.GroupStepSize;
            }

            result.AddRange(child.GetShape(rand, proto, offset, size, stepSize));
        }

        return result.Distinct().ToList();
    }
}

[DataDefinition]
public partial record struct GroupEntityShapeOptions
{
    [DataField]
    public Vector2? Offset;

    [DataField]
    public int? GroupSize;

    [DataField]
    public int? GroupStepSize;
}
