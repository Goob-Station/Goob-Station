using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// This handles spawning one, or multiple entities in the general vicinity of the entity it is attached to.
/// It's basically a more generic version of VoidPortalComponent from Wraith.
/// </summary>
[RegisterComponent]
public sealed partial class VicinitySpawnerComponent : Component
{
    /// <summary>
    /// Interval between spawn attempts.
    /// </summary>
    [DataField]
    public TimeSpan SpawnInterval = TimeSpan.FromSeconds(0.5);

    [DataField]
    public TimeSpan Accumulator = TimeSpan.Zero;

    /// <summary>
    /// How many entities to spawn per spawn interval.
    /// </summary>
    [DataField]
    public int NumberToSpawn = 1;

    /// <summary>
    /// List of mobs that can be summoned.
    /// </summary>
    [DataField(required: true)]
    public List<EntProtoId> Prototype = new();

    /// <summary>
    /// Empty prototype that serves for checking if a spot is obstructed or not.
    /// </summary>
    [DataField]
    public EntProtoId EmptyPrototype = "VoidPortalEmpty";

    /// <summary>
    /// Offset range which controls how far away the entities can be spawned away from the component holder.
    /// </summary>
    [DataField]
    public int OffsetForSpawn;
}

