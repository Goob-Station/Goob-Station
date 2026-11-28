using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Spawners;

/// <summary>
/// This handles spawning one, or multiple entities in the general vicinity of the entity it is attached to.
/// It's basically a more generic version of VoidPortalComponent from Wraith.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class VicinitySpawnerComponent : Component
{
    /// <summary>
    /// Time in-between the spawning.
    /// </summary>
    [DataField]
    public TimeSpan SpawnInterval = TimeSpan.FromSeconds(0.5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextSpawn = TimeSpan.Zero;

    /// <summary>
    /// How many entities to spawn per SpawnInterval.
    /// </summary>
    [DataField]
    public int NumberToSpawn = 1;

    /// <summary>
    /// List of prototypes that will be spawned.
    /// </summary>
    [DataField(required: true)]
    public List<EntProtoId> Prototypes = default!;

    /// <summary>
    /// Offset for how far away the entities can be spawned away from the spawner.
    /// </summary>
    [DataField]
    public int Range;
}
