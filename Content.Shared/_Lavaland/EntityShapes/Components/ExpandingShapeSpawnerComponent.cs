using System.Numerics;
using Content.Shared._Lavaland.EntityShapes.Shapes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.EntityShapes.Components;

/// <summary>
/// Spawns an entity shape periodically or with a delay. Can be modified to expand, shrink, or move with time.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ExpandingShapeSpawnerComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public EntityShape Shape;

    [DataField, AutoNetworkedField]
    public EntProtoId Spawn;

    [DataField, AutoNetworkedField]
    public Vector2? CounterOffset;

    [DataField, AutoNetworkedField]
    public float? CounterSize;

    [DataField, AutoNetworkedField]
    public float? CounterStepSize;
}
