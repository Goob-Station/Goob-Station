using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Spawn a safe zone indicator in a random location near this component holder.
/// Using that safe zone's coordinates as a base, rings of damaging beams will spawn in and begin closing in
/// by reducing radius and spawning closer to the safe zone's position, until they reach the minimum radius allowed.
/// </summary>

[RegisterComponent]
public sealed partial class ORTConvergenceComponent : Component
{
    /// <summary>
    /// The warning prototype.
    /// </summary>
    [DataField]
    public EntProtoId WarningPrototype = "ORTWarningBox";

    /// <summary>
    /// The safe zone prototype.
    /// </summary>
    [DataField]
    public EntProtoId SafeZonePrototype = "ORTSafeZoneIndicator";

    /// <summary>
    /// Radius of the safe zone. Beams stop spawning at this radius.
    /// </summary>
    [DataField]
    public float SafeZoneRadius = 2f;

    /// <summary>
    /// Starting radius of the beam ring.
    /// </summary>
    [DataField]
    public float StartRadius = 12f;

    /// <summary>
    /// How many beams to spawn per ring.
    /// </summary>
    [DataField]
    public int Count = 48; // If this is causing lag in real servers then obv lower it. alternatively lower the startradius so it doesnt need this many

    /// <summary>
    /// Minimum beams per ring.
    /// </summary>
    [DataField]
    public int MinCount = 8;

    /// <summary>
    /// Delay between the safe zone spawning and the first wave.
    /// </summary>
    [DataField]
    public TimeSpan InitialDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Delay between each wave of beams.
    /// </summary>
    [DataField]
    public TimeSpan WaveDelay = TimeSpan.FromSeconds(0.25f);

    /// <summary>
    /// How many waves to spawn before it reaches the safe zone.
    /// </summary>
    [DataField]
    public int WaveCount = 10;

    /// <summary>
    /// Minimum distance from UID to spawn.
    /// </summary>
    [DataField]
    public float MinDistance = 4f;

    /// <summary>
    /// Maximum distance from UID to spawn.
    /// </summary>
    [DataField]
    public float MaxDistance = 10f;

    public bool Active;
    public TimeSpan NextWaveTime;
    public int CurrentWave;
    public EntityUid? SafeZoneEntity;
}
