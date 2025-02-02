using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System;

namespace Content.Shared.Disease;

/// <summary>
/// Component for disease behaviors
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseEffectComponent : Component
{
    /// <summary>
    /// Strength of this effect
    /// Changes on disease mutation
    /// </summary>
    [DataField]
    public float Severity = 1f;

    [DataField]
    public float Complexity = 4f;
}

/// <summary>
/// Base component for disease effects and conditions
/// </summary>
public abstract partial class BaseDiseaseEffect : Component
{
    /// <summary>
    /// Whether this effect or condition should scale from disease severity
    /// </summary>
    [DataField]
    public bool SeverityScale = true;

    /// <summary>
    /// Whether this effect or condition should scale from the update interval
    /// Use for effects that happen based on time passage conditions
    /// </summary>
    [DataField]
    public bool TimeScale = true;
}

/// <summary>
/// Deals damage over time to host
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseDamageEffectComponent : BaseDiseaseEffect
{
    [DataField]
    public DamageSpecifier Damage = default!;
}

/// <summary>
/// Decrease immunity progress on disease, use for incurable-once-developed diseases
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseFightImmunityEffectComponent : BaseDiseaseEffect
{
    [DataField]
    public float Amount = -0.04f;
}

/// <summary>
/// Causes the host to vomit
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseVomitEffectComponent : BaseDiseaseEffect
{
    [DataField]
    public float ThirstChange = -40f;

    [DataField]
    public float FoodChange = -40f;
}

/// <summary>
/// Causes this effect to only trigger ocassionally
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DiseasePeriodicConditionComponent : BaseDiseaseEffect
{
    /// <summary>
    /// Minimum delay between passes, increases inversely proportional to severity
    /// </summary>
    [DataField]
    public TimeSpan DelayMin = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Maximum delay between passes, increases inversely proportional to severity
    /// </summary>
    [DataField]
    public TimeSpan DelayMax = TimeSpan.FromSeconds(30);

    // state: time since last passed
    [ViewVariables, AutoNetworkedField]
    public TimeSpan TimeSinceLast = TimeSpan.FromSeconds(0);

    // state: delay until next pass
    [ViewVariables, AutoNetworkedField]
    public TimeSpan? CurrentDelay;
}
