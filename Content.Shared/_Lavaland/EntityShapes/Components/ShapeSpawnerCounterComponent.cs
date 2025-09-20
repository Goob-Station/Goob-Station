using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.EntityShapes.Components;

/// <summary>
/// Used for different shape spawner components to count new steps for spawns.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShapeSpawnerCounterComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan SpawnPeriod = TimeSpan.FromSeconds(1f);

    [DataField, AutoNetworkedField]
    public int MaxCounter = 1;

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextSpawn;

    [ViewVariables, AutoNetworkedField]
    public int Counter;
}
