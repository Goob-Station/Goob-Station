using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.EntityShapes.Components;

/// <summary>
/// Scales <see cref="ExpandingShapeSpawnerComponent"/> with anger
/// of an owner that spawned this EntityShapeSpawner.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AngerShapeSpawnerComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2i CounterRange;

    [DataField, AutoNetworkedField]
    public Vector2 SpawnPeriodRange;
}
