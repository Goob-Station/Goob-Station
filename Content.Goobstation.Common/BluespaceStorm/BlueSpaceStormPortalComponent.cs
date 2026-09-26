using Robust.Shared.Prototypes;


namespace Content.Goobstation.Common.BlueSpaceStorm;

[RegisterComponent]
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

    /// <summary>
    /// Mobs to be spawned
    /// </summary>
    [DataField]
    public List<EntProtoId> MobsToSpawn = [];

    [DataField]
    public List<EntityUid> SpawnedMobs = [];


}
