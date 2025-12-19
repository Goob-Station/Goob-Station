using Content.Shared.Chat.Prototypes;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease.Components;

/// <summary>
/// Component for disease behaviors
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[EntityCategory("Diseases")]
public sealed partial class DiseaseEffectComponent : Component
{
    /// <summary>
    /// Strength of this effect
    /// Changes on disease mutation
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Severity = 1f; // don't bring this outside of expected bounds or viro will probably choke and die

    /// <summary>
    /// Minimum severity this effect can have
    /// Used to prevent diseases with 15 morbillion 0.005 severity effects
    /// </summary>
    [DataField]
    public float MinSeverity = 0.1f;

    /// <summary>
    /// Contribution of this effect to disease complexity
    /// Actual impact scales with severity
    /// </summary>
    [DataField]
    public float Complexity = 10f;

    /// <summary>
    /// Get the complexity this effect is currently contributing.
    /// </summary>
    public float GetComplexity()
    {
        return Complexity * Severity;
    }
}

/// <summary>
/// Base component for disease effects and conditions for which it makes sense to choose scaling off severity, time, or progress
/// </summary>
public abstract partial class ScalingDiseaseEffect : Component
{
    /// <summary>
    /// Whether this effect or condition should scale from effect severity
    /// </summary>
    [DataField]
    public bool SeverityScale = true;

    /// <summary>
    /// Whether this effect or condition should scale from the update interval
    /// Use for effects that do their action over time as opposed to just setting something
    /// </summary>
    [DataField]
    public bool TimeScale = true;

    /// <summary>
    /// Whether this effect or condition should scale from the progress of the host disease
    /// </summary>
    [DataField]
    public bool ProgressScale = true;
}

/// <summary>
/// Decrease immunity progress on disease, use for incurable-once-developed diseases
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseFightImmunityEffectComponent : ScalingDiseaseEffect
{
    [DataField]
    public float Amount = -0.04f;
}

/// <summary>
/// Causes a spread effect of specified shape and type
/// For use with conditions
/// Scaling affects infection chance
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseSpreadEffectComponent : ScalingDiseaseEffect
{
    /// <summary>
    /// Angle in front of the entity to check for infectables
    /// </summary>
    [DataField]
    public Angle Arc = Angle.FromDegrees(120);

    /// <summary>
    /// Up to how far away entities to check
    /// </summary>
    [DataField]
    public float Range = 2f;

    [DataField]
    public DiseaseSpreadSpecifier SpreadParams = new(1f, 1f, "Debug");
}

/// <summary>
/// Causes a forced spread effect of specified shape, will infect EVERYTHING
/// For use with conditions
/// Note: this is a debug effect and should not be used in real diseases
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseForceSpreadEffectComponent : ScalingDiseaseEffect
{
    /// <summary>
    /// Up to how far away entities to check
    /// </summary>
    [DataField]
    public float Range = 2f;

    [DataField]
    public float Chance = 1f;

    /// <summary>
    /// Whether to ensure StatusIconComponent on the spread-to entities for viro hud to work properly.
    /// </summary>
    [DataField]
    public bool AddIcon = true;

    [DataField]
    public DiseaseSpreadSpecifier SpreadParams = new(1f, 1f, "Debug");
}

/// <summary>
/// Causes the host to vomit
/// For use with conditions
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseVomitEffectComponent : ScalingDiseaseEffect
{
    // maybe split thirst/food decrease and actual vomiting into separate effects?
    [DataField]
    public float ThirstChange = -40f;

    [DataField]
    public float FoodChange = -40f;
}

/// <summary>
/// Causes the host to get flashed
/// For use with conditions
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseFlashEffectComponent : ScalingDiseaseEffect
{
    /// <summary>
    /// The duration to flash for
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(2f);

    /// <summary>
    /// How much to slow the host down during the flash
    /// </summary>
    [DataField]
    public float SlowTo = 0.8f;

    /// <summary>
    /// For how much to stun the host, if not null
    /// </summary>
    [DataField]
    public TimeSpan? StunDuration;
}

/// <summary>
/// Causes a popup to happen
/// For use with conditions
/// </summary>
[RegisterComponent]
public sealed partial class DiseasePopupEffectComponent : Component
{
    [DataField]
    public LocId String = "disease-effect-popup-default";

    [DataField("popupType")]
    public PopupType Type = PopupType.SmallCaution;

    /// <summary>
    /// Whether only the host should see the popup
    /// </summary>
    [DataField]
    public bool HostOnly = true;
}

/// <summary>
/// Tries to pry a tile in range
/// </summary>
[RegisterComponent]
public sealed partial class DiseasePryTileEffectComponent : Component
{
    /// <summary>
    /// Up to how far away entities to check
    /// </summary>
    [DataField]
    public float Range = 2f;

    /// <summary>
    /// How many times to re-sample for a tile if we fail
    /// </summary>
    [DataField]
    public int Attempts = 1;
}

/// <summary>
/// Causes audio to play at the host
/// For use with conditions
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseAudioEffectComponent : Component
{
    [DataField]
    public SoundCollectionSpecifier Sound;

    /// <summary>
    /// If not null, will use this sound collection for female characters instead
    /// </summary>
    [DataField]
    public SoundCollectionSpecifier? SoundFemale = null;
}

/// <summary>
/// Causes the host to emote
/// For use with conditions
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseEmoteEffectComponent : Component
{
    [DataField]
    public ProtoId<EmotePrototype> Emote;

    [DataField]
    public bool WithChat = true;
}

/// <summary>
/// Causes the target component to have set fields have their chosen fields set to a multiple of the default specified according to effect application severity.
/// Can be used to have, say, gravity well component scale its acceleration from the effect severity.
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseGenericEffectComponent : ScalingDiseaseEffect
{
    /// <summary>
    /// Component to act on.
    /// </summary>
    [DataField(required: true)]
    public string Component = string.Empty;

    /// <summary>
    /// Fields to set.
    /// String is field, key is max value.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, float> Defaults = new();

    /// <summary>
    /// Whether to apply a severity of zero when conditions to executing the effect fail.
    /// </summary>
    [DataField]
    public bool ZeroOnFail = true;
}
