using System.Numerics;
using Content.Shared._Lavaland.EntityShapes.Shapes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Tile.Components;

/// <summary>
/// Spawns an entity shape periodically or with a delay. Can be modified to expand, shrink, or move with time.
/// </summary>
[RegisterComponent]
public sealed partial class ExpandingShapeSpawnerComponent : Component
{
    [DataField(required: true)]
    public EntityShape Shape;

    [DataField]
    public EntProtoId Spawn;

    [DataField]
    public float SpawnPeriod = 1f;

    [DataField]
    public int MaxCounter = 1;

    [DataField]
    public Vector2? CounterOffset;

    [DataField]
    public float? CounterSize;

    [DataField]
    public float? CounterStepSize;

    [ViewVariables]
    public float Accumulator;

    [ViewVariables]
    public int Counter;
}
