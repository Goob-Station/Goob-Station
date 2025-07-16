using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Tile;

/// <summary>
/// Can contain multiple just one or multiple TileShapes
/// to be used in components and code more conveniently.
/// </summary>
[Prototype]
public sealed partial class TileShapePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public bool CanOverlap;

    [DataField(required: true)]
    public TileShape[] Shapes = default!;
}
