using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components;

/// <summary>
/// Tracks a bloodsucker's humanity score.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodsuckerHumanityComponent : Component
{
    /// <summary>
    /// Maximum possible humanity.
    /// </summary>
    [DataField]
    public float MaxHumanity = 100f;

    /// <summary>
    /// Current humanity value.  Clamped to 0.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CurrentHumanity = 100f;

    /// <summary>
    /// Frenzy threshold (blood units) when humanity is at maximum.
    /// At max humanity frenzy triggers latest (lowest blood value).
    /// </summary>
    [DataField]
    public float FrenzyThresholdAtMaxHumanity = 75f;

    /// <summary>
    /// Frenzy threshold (blood units) when humanity is at zero.
    /// At zero humanity frenzy triggers earliest (highest blood value).
    /// </summary>
    [DataField]
    public float FrenzyThresholdAtZeroHumanity = 300f;

    /// <summary>
    /// Burn damage per second taken during frenzy at maximum humanity.
    /// </summary>
    [DataField]
    public float FrenzyBurnDamageAtMaxHumanity = 1f;

    /// <summary>
    /// Burn damage per second taken during frenzy at zero humanity.
    /// </summary>
    [DataField]
    public float FrenzyBurnDamageAtZeroHumanity = 8f;

    /// <summary>
    /// Minimum humanity required to use gated actions
    /// </summary>
    [DataField]
    public float GatedActionMinHumanity = 30f;
}
