using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

/// <summary>
/// Configuration for the Feed action.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerFeedComponent : Component
{
    /// <summary>Blood volume (units) transferred from target to vampire per drain tick.</summary>
    [DataField]
    public float BloodDrainAmount = 10f;

    /// <summary>Initial do-after delay before the first drain tick fires (seconds).</summary>
    [DataField]
    public float StartDelay = 2f;

    /// <summary>
    /// How long the target sleeps when fed upon while held in an aggressive grab (seconds).
    /// </summary>
    [DataField]
    public float SleepDuration = 60f;

    /// <summary>Blood cost (units) deducted from the vampire's bloodstream to activate.</summary>
    [DataField]
    public float BloodCost = 0f;

    /// <summary>Humanity lost when activating this action. 0 = no cost.</summary>
    [DataField]
    public float HumanityCost = 0f;

    /// <summary>If true, this action cannot be started while the vampire is in frenzy.</summary>
    [DataField]
    public bool DisabledInFrenzy = false;
}
