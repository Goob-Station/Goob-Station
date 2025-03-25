using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.RandomChanceSpawner;

/// <summary>
/// Shitcode my beloved 
/// </summary>
[RegisterComponent, EntityCategory("Spawner")]
public sealed partial class RandomChanceSpawnerComponent : Component
{
    [DataField]
    public Dictionary<EntProtoId, float> ToSpawn = new();
}
