using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Content.Shared.Disease;

/// <summary>
/// Component for disease behaviors
/// </summary>
[RegisterComponent]
[EntityCategory("Diseases")]
public sealed partial class DiseaseEffectComponent : Component
{
    /// <summary>
    /// Strength of this effect
    /// Changes on disease mutation
    /// </summary>
    [DataField]
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
    /// Disease types allowed to naturally roll this effect
    /// </summary>
    [DataField]
    public HashSet<ProtoId<DiseaseTypePrototype>> AllowedDiseaseTypes = new();

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
/// Deals damage over time to host
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseDamageEffectComponent : ScalingDiseaseEffect
{
    [DataField]
    public DamageSpecifier Damage = default!;
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
    [DataField]
    public ProtoId<DiseaseSpreadPrototype> SpreadType;

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

    /// <summary>
    /// Power of the infection attempt, determines how well it gets through infection protection
    /// </summary>
    [DataField]
    public float InfectionPower = 1f;

    /// <summary>
    /// If the infection attempt gets through, chance for it to actually work
    /// </summary>
    [DataField]
    public float InfectionChance = 0.2f;
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
    public string String = "disease-effect-popup-default";

    [DataField]
    public PopupType Type = PopupType.SmallCaution;

    /// <summary>
    /// Whether only the host should see the popup
    /// </summary>
    [DataField]
    public bool HostOnly = true;
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

