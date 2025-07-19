using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Tile.Shapes;

/// <summary>
/// Represents a shape made out of multiple tiles.
/// </summary>
[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class TileShape
{
    /// <summary>
    /// If specified, will add this shape into a shapes group,
    /// that can be customized via <see cref="GroupTileShape"/>.
    /// That way you can change size or offset for groups of tiles
    /// instead of individually changing values.
    /// </summary>
    [DataField]
    public string? OverrideGroup;

    [DataField("offset")]
    public Vector2i DefaultOffset;

    [DataField("size")]
    public int DefaultSize;

    [ViewVariables]
    public Vector2i Offset;

    [ViewVariables]
    public int Size;

    /// <summary>
    /// Calculates this shape and also lets you customize some parameters of shape's generation.
    /// </summary>
    public List<Vector2i> GetShape(
        System.Random rand,
        IPrototypeManager proto,
        Vector2i? center = null,
        int? size = null)
    {
        Offset = DefaultOffset + center ?? Vector2i.Zero;
        Size = size ?? DefaultSize;
        return GetShapeImplementation(rand, proto);
    }

    protected abstract List<Vector2i> GetShapeImplementation(System.Random rand, IPrototypeManager proto);
}
