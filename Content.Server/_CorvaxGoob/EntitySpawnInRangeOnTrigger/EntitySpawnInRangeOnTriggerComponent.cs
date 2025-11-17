using Robust.Shared.Prototypes;

namespace Content.Server._CorvaxGoob.EntitySpawnInRangeOnTrigger;

[RegisterComponent]
public sealed partial class EntitySpawnInRangeOnTriggerComponent : Component
{
    [DataField]
    public List<EntitySpawnInRangeSettingsEntry> Entries = new();

    [DataField]
    public EntProtoId? SpawnEffect = "PuddleSparkle";
}

[DataRecord]
public partial record struct EntitySpawnInRangeSettingsEntry()
{
    /// <summary>
    /// A list of entities that are random picked to be spawned on trigger
    /// </summary>
    public List<EntProtoId> Spawns { get; set; } = new();

    public EntitySpawnSettings Settings { get; set; } = new();
}

[DataRecord]
public partial record struct EntitySpawnSettings()
{
    /// <summary>
    /// should entities block spawning?
    /// </summary>
    public bool CanSpawnOnEntities { get; set; } = false;

    /// <summary>
    /// The minimum number of entities that spawn per pulse
    /// </summary>
    public int MinAmount { get; set; } = 0;

    /// <summary>
    /// The maximum number of entities that spawn per pulse
    /// scales with severity.
    /// </summary>
    public int MaxAmount { get; set; } = 1;

    /// <summary>
    /// The distance from the object in which the entities will not appear
    /// </summary>
    public float MinRange { get; set; } = 0f;

    /// <summary>
    /// The maximum radius the entities will spawn in.
    /// </summary>
    public float MaxRange { get; set; } = 1f;
}
