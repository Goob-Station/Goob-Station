using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Tendril.Components;

[RegisterComponent]
public sealed partial class TendrilComponent : Component
{
    [DataField]
    public int MaxSpawns = 3;

    /// <summary>
    /// When this amount of mobs is killed, tendril breaks.
    /// </summary>
    [DataField]
    public int MobsToDefeat = 5;

    [ViewVariables]
    public int DefeatedMobs = 0;

    [DataField]
    public float SpawnDelay = 10f;

    [DataField]
    public float ChasmDelay = 5f;

    [DataField]
    public float ChasmDelayOnMobsDefeat = 15f;

    [DataField]
    public int ChasmRadius = 2;

    [DataField(required: true)]
    public List<EntProtoId> Spawns = [];

    [ViewVariables]
    public List<EntityUid> Mobs = [];

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastSpawn = TimeSpan.Zero;

    [ViewVariables]
    public bool DestroyedWithMobs;

    [ViewVariables]
    public float UpdateAccumulator;

    [DataField]
    public float UpdateFrequency = 5;
}
