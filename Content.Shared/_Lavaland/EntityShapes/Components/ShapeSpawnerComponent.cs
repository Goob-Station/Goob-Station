using Content.Shared._Lavaland.EntityShapes.Shapes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Tile.Components;

/// <summary>
/// Spawns an entity shape on MapInit.
/// </summary>
[RegisterComponent]
public sealed partial class ShapeSpawnerComponent : Component
{
    [DataField(required: true)]
    public EntityShape Shape;

    [DataField]
    public EntProtoId Spawn;
}
