using JetBrains.Annotations;

namespace Content.Shared._Lavaland.Tile;

/// <summary>
/// Represents a shape made out of multiple tiles.
/// </summary>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class TileShape
{
    public virtual string Name => GetType().Name;

    public abstract List<Vector2i> GetShape(Vector2i center);
}
