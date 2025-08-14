using Content.Shared._Lavaland.EntityShapes.Shapes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes.Components;

/// <summary>
/// Spawns different shapes in order
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SequenceShapeSpawnerComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public Dictionary<int, EntityShape> Scheduler;

    [DataField, AutoNetworkedField]
    public EntProtoId Spawn;
}
