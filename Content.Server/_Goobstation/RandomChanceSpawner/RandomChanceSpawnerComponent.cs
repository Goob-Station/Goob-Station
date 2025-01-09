using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.RandomChanceSpawner;

/// <summary>
/// Shitcode my beloved 
/// </summary>
[RegisterComponent, EntityCategory("Spawner")]
public sealed partial class RandomChanceSpawnerComponent : Component
{
    [DataField]
    public Dictionary<EntProtoId, float> ToSpawn = new();
}
