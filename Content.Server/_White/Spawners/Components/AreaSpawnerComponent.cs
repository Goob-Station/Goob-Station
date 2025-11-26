using Robust.Shared.Prototypes;

namespace Content.Server._White.Spawners.Components;

[RegisterComponent]
public sealed partial class AreaSpawnerComponent : Component
{
    [DataField]
    public int Radius = 3;

    [DataField]
    public EntProtoId SpawnPrototype;

    [DataField]
    public TimeSpan SpawnDelay = TimeSpan.FromSeconds(3);

    [DataField]
    public float MinTime = 1f;

    [DataField]
    public float MaxTime = 5f;

    [ViewVariables]
    public TimeSpan SpawnAt = TimeSpan.Zero;

    [ViewVariables]
    public List<EntityUid> Spawneds = new List<EntityUid>();
}
