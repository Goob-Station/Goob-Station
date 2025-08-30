using Content.Shared._Lavaland.EntityShapes.Shapes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes.Components;

/// <summary>
/// Spawns an entity shape on MapInit.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShapeSpawnerComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public EntityShape Shape;

    [DataField(required: true), AutoNetworkedField]
    public EntProtoId Spawn;
}
