using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Berserker;

/// <summary>
/// Increases damage/speed of entity the lower its health is.
/// Optionally can also grant components at certain health threshholds.
/// These get reverted if you get healed.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BerserkerStatsComponent : Component
{
    /// <summary>
    /// Should melee damage increase with damage?
    /// </summary>
    [DataField]
    public bool ScaleDamage = true;

    /// <summary>
    /// Should movement speed increase with damage?
    /// </summary>
    [DataField]
    public bool ScaleSpeed;

    /// <summary>
    /// Damage multiplier at full health.
    /// </summary>
    [DataField]
    public float DamageMinMultiplier = 1f;

    /// <summary>
    /// Damage multiplier cap. Basically only hits when you're dead.
    /// </summary>
    [DataField]
    public float DamageMaxMultiplier = 2.5f;

    /// <summary>
    /// Speed multiplier at full health.
    /// </summary>
    [DataField]
    public float SpeedMinMultiplier = 1f;

    /// <summary>
    ///  Speed multiplier cap. Basically only hits when you're dead.
    /// </summary>
    [DataField]
    public float SpeedMaxMultiplier = 2.5f;

    /// <summary>
    /// If true, multiplier becomes exponential rather than linear.
    /// That is to say, you better run, son.
    /// </summary>
    [DataField]
    public bool ExponentialScaling;

    /// <summary>
    /// If ExponentialScaling is true, this is how quickly to scale it.
    /// 1 is linear (and kinda pointless), losing 10% health gets you 10% more stats.
    /// Any number above 1 means it starts scaling slowly but gets faster later.
    /// Any number below 1 means it scales fast at first but less later.
    /// </summary>
    [DataField]
    public float ScalingExponent = 2f;

    /// <summary>
    /// Grant component based on specified health thresholds.
    /// If healed above that threshold the component gets removed.
    /// </summary>
    [DataField]
    public List<BerserkerComponentThreshold> ComponentThresholds = new();

    /// <summary>
    /// Currently active thresholds.
    /// </summary>
    [ViewVariables]
    public HashSet<int> ActiveThresholds = new();

    /// <summary>
    /// Self-explanatory.
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public float CurrentDamageMultiplier = 1f;

    /// <summary>
    /// Self-explanatory.
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public float CurrentSpeedMultiplier = 1f;
}

/// <summary>
/// Component to add and health threshold to add it at.
/// </summary>
[DataDefinition]
public sealed partial class BerserkerComponentThreshold
{
    /// <summary>
    /// 1 = full HP, 0 = dead. So do stuff like 0.25 and etc.
    /// </summary>
    [DataField(required: true)]
    public float HealthThreshold;

    [DataField(required: true)]
    public ComponentRegistry Components = new();
}
