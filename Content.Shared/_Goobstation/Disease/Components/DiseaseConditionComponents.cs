using Content.Shared.Disease;
using Robust.Shared.GameStates;
using System;

namespace Content.Shared.Disease;

/// <summary>
/// Causes this effect to only trigger ocassionally
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DiseasePeriodicConditionComponent : ScalingDiseaseEffect
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

/// <summary>
/// Causes this effect to only trigger at specific disease progress levels
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseProgressConditionComponent : Component
{
    /// <summary>
    /// Minimum disease progress for this effect to be active, doesn't apply if null
    /// </summary>
    [DataField]
    public float? MinProgress;

    /// <summary>
    /// Maximum disease progress for this effect to be active, doesn't apply if null
    /// </summary>
    [DataField]
    public float? MaxProgress;
}
