using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class VoidPortalComponent : Component
{
    /// <summary>
    /// Interval between spawn attempts (one wave).
    /// </summary>
    [DataField]
    public TimeSpan SpawnInterval = TimeSpan.FromSeconds(30);

    [DataField]
    public TimeSpan Accumulator = TimeSpan.Zero;

    /// <summary>
    /// Current power points available for spawning.
    /// </summary>
    [DataField]
    public int CurrentPower;

    /// <summary>
    /// Maximum amount of points the portal can have at any one time.
    /// Set to <= 0 to disable the cap.
    /// </summary>
    [DataField]
    public int MaxPower = 30;

    /// <summary>
    /// Base amount of power gained each wave.
    /// </summary>
    [DataField]
    public int PowerGainPerTick = 5;

    /// <summary>
    /// Additional power added per wave (e.g. wave 0 +5, wave 1 +10, wave 2 +15).
    /// </summary>
    [DataField]
    public int ExtraPower = 5;

    /// <summary>
    /// Number of completed spawn waves. Used to scale power growth.
    /// </summary>
    [DataField]
    public int WavesCompleted;

    /// <summary>
    /// Weighted list of mobs and their point cost.
    /// Each entry is a prototype + its cost.
    /// </summary>
    [DataField]
    public List<MobSpawnEntry> MobEntries = new();

    [DataField]
    public EntityUid? PortalOwner;
}

/// <summary>
/// Represents one possible mob spawn option with a cost.
/// </summary>
[DataDefinition]
public sealed partial class MobSpawnEntry
{
    [DataField(required: true)]
    public EntProtoId Prototype;

    [DataField(required: true)]
    public int Cost;
}
