using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Components;

/// <summary>
/// Spawns a ring of orbiting entities parented to this entity.
/// </summary>
[RegisterComponent]
public sealed partial class OrbitingRingComponent : Component
{
    /// <summary>
    /// How far out the ring expands before stopping.
    /// </summary>
    [DataField]
    public float RingDistance = 2f;

    /// <summary>
    /// How quickly the entities grow outward.
    /// </summary>
    [DataField]
    public float GrowSpeed = 1f;

    /// <summary>
    /// How many entities to spawn in the ring.
    /// </summary>
    [DataField]
    public int Count = 7;

    /// <summary>
    /// Prototype to spawn.
    /// </summary>
    [DataField]
    public EntProtoId Prototype;

    /// <summary>
    /// Sound played when the ring is spawned.
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound;

    /// <summary>
    /// Currently spawned ring entities.
    /// </summary>
    public List<EntityUid> Entities = new();
}
