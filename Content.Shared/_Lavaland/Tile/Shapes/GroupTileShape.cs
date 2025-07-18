using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Tile.Shapes;

/// <summary>
/// Tile shape that is made of multiple other shapes.
/// </summary>
public sealed partial class GroupTileShape : TileShape
{
    [DataField(required: true)]
    public List<TileShape> Children = new();

    [DataField]
    public Dictionary<string, GroupTileShapeOptions>? Options = new();

    protected override List<Vector2i> GetShapeImplementation(System.Random rand, IPrototypeManager proto)
    {
        var result = new List<Vector2i>();
        foreach (var child in Children)
        {
            Vector2i? offset = null;
            int? size = null;

            if (Options != null
                && child.OverrideGroup != null
                && Options.TryGetValue(child.OverrideGroup, out var options))
            {
                offset = options.Offset;
                size = options.GroupSize;
            }

            result.AddRange(child.GetShape(rand, proto, offset ?? Offset, size ?? Size));
        }

        return result.ToHashSet().ToList();
    }
}

[DataDefinition]
public partial record struct GroupTileShapeOptions
{
    [DataField]
    public Vector2i Offset;

    [DataField]
    public int GroupSize;
}
