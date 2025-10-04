using Content.Shared._Lavaland.EntityShapes.Shapes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes.Components;

/// <summary>
/// Spawns an entity shape on MapInit.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShapeSpawnerComponent : Component
{
    [DataField(required: true)]
    public EntityShape Shape;

    [DataField(required: true)]
    public EntProtoId Spawn;
}
