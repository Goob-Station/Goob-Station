using Content.Server.Spawners.EntitySystems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Spawners.Components;

/// <summary>
/// When a <c>TimedDespawnComponent"</c> despawns, another one will be spawned in its place.
/// </summary>
[RegisterComponent, Access(typeof(SpawnOnDespawnSystem))]
public sealed partial class SpawnOnDespawnComponent : Component
{
    /// <summary>
    /// Entity prototype to spawn.
    /// </summary>
    [DataField]
    public EntProtoId? Prototype; // Lavaland Change

    // Lavaland Change start - make it spawn more. without specific intrusion.
    [DataField]
    public List<EntProtoId> Prototypes = [];
    // Lavaland Change end
}
