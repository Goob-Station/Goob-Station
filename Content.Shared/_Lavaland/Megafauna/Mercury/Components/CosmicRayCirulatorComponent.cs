using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

[RegisterComponent]
public sealed partial class CosmicRayCirculatorComponent : Component
{
    /// <summary>
    /// Warning that spawns before the real entity.
    /// </summary>
    [DataField]
    public EntProtoId WarningPrototype;

    /// <summary>
    /// Radius of ring entities are spawned in.
    /// </summary>
    [DataField]
    public float Radius = 3f;

    /// <summary>
    /// How many entities to spawn.
    /// </summary>
    [DataField]
    public int Count = 9;

    /// <summary>
    /// Delay between warning and real entity spawning.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Delay between each expanding wave.
    /// </summary>
    [DataField]
    public TimeSpan WaveDelay = TimeSpan.FromSeconds(0.15f);

    /// <summary>
    /// How many expanding waves to spawn.
    /// </summary>
    [DataField]
    public int WaveCount = 10;

    /// <summary>
    /// How much to increase radius per wave.
    /// </summary>
    [DataField]
    public float RadiusIncrease = 1f;

    /// <summary>
    /// Action is being used to hold still.
    /// </summary>
    public bool Active;

    /// <summary>
    /// Time at which the next wave spawns.
    /// </summary>
    public TimeSpan NextWaveTime;

    /// <summary>
    /// The wave at which it currently is.
    /// </summary>
    public int CurrentWave;
}
