using Content.Shared._Lavaland.Tile.Shapes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Tile;

/// <summary>
/// Contains one or multiple TileShapes to create a pattern.
/// </summary>
[Prototype]
public sealed partial class TileShapePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public TileShape Shape = default!;
}
