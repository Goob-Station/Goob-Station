using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Common.BluespaceStorm;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class BluespaceStormPortalComponent : Component
{
    /// <summary>
    /// Time until mobs spawn (seconds)
    /// </summary>
    [DataField]
    public int TimeForSpawn = 55;

    /// <summary>
    /// Time between pulses (seconds)
    /// </summary>
    [DataField]
    public int TimeForPulse = 10;

    [DataField]
    public List<BluespaceStormPortalPulseEffectOption> PulseEffects = [];

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextMobSpawnTime;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextPulseTime;

    /// <summary>
    /// Mob prototypes selected randomly when this portal opens
    /// </summary>
    [DataField]
    public List<EntProtoId> MobSpawnPool = [];

    /// <summary>
    /// Number of mobs selected from the spawn pool
    /// </summary>
    [DataField]
    public int MobSpawnCount = 0;

    public List<EntProtoId> MobsToSpawn = [];

    [DataField]
    public List<EntityUid> SpawnedMobs = [];

    public bool MobsSpawned;

    public bool MobsAllDeadEventRaised;
}

[DataDefinition]
public sealed partial class BluespaceStormPortalPulseEffectOption
{
    [DataField(required: true)]
    public int Weight;

    [DataField(required: true)]
    public string Effect = string.Empty;
}
