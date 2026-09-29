using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Common.BlueSpaceStorm;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class BlueSpaceStormPortalComponent : Component
{
    /// <summary>
    /// Portal type
    /// </summary>
    [DataField]
    public EntProtoId PortalType;

    /// <summary>
    /// Time until mobs spawn (seconds)
    /// </summary>
    [DataField]
    public int TimeForSpawn = 60;

    /// <summary>
    /// Time between pulses (seconds)
    /// </summary>
    [DataField]
    public int TimeForPulse = 10;

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
