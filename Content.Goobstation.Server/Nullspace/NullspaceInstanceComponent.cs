using System.Numerics;

namespace Content.Goobstation.Server.Nullspace;

[RegisterComponent, Access(typeof(NullspaceSystem))]
public sealed partial class NullspaceInstanceComponent : Component
{
    [ViewVariables]
    public readonly HashSet<EntityUid> Players = new();

    [ViewVariables]
    public EntityUid? Clown;

    [ViewVariables]
    public float ClownSpawnAt;

    [ViewVariables]
    public TimeSpan? ClownResumeAt;

    public Vector2 ClownCheckPosition;

    public TimeSpan ClownCheckAt;

    public readonly HashSet<Vector2i> Anchors = new();

    public readonly Dictionary<Vector2i, NullspaceChunk> Chunks = new();

    public readonly Queue<NullspacePendingSpawn> PendingSpawns = new();

    public readonly Queue<NullspaceUnload> PendingUnloads = new();

    public int ChunkGeneration;

    public readonly HashSet<Vector2i> DisturbedLights = new();

    public TimeSpan SilenceCheckAt;

    public TimeSpan NextShiftAt;
}

public sealed class NullspaceChunk(int generation)
{
    public readonly int Generation = generation;
    public readonly List<EntityUid> Entities = new();
}

public readonly record struct NullspacePendingSpawn(Vector2i Chunk, int Generation, Vector2i Cell);

public sealed class NullspaceUnload(Vector2i chunk, List<EntityUid> entities)
{
    public readonly Vector2i Chunk = chunk;
    public readonly List<EntityUid> Entities = entities;
}
