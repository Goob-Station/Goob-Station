using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Nullspace;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NullspaceMapComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Seed;

    [DataField, AutoNetworkedField]
    public TimeSpan? StartTime;

    [DataField, AutoNetworkedField]
    public HashSet<Vector2i> DeadLights = new();

    [DataField, AutoNetworkedField]
    public Dictionary<Vector2i, int> ChunkVariants = new();

    [DataField, AutoNetworkedField]
    public int VariantVersion;

    [ViewVariables]
    public NullspaceMaze? Maze;
}

