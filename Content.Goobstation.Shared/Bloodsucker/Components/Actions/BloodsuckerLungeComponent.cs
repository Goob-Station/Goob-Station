using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

/// <summary>
/// Configuration for the predatory lunge action.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerLungeComponent : Component
{

    [DataField]
    public NetEntity CurrentTarget = NetEntity.Invalid;

    /// <summary>
    /// Initial do-after delay before lunging.
    /// </summary>
    [DataField]
    public float StartDelay = 2f;

    /// <summary>
    /// How quickly to move towards the target.
    /// </summary>
    public float DashSpeed = 8f;

    #region Generic

    /// <summary>
    /// The current level of this action.
    /// </summary>
    public int ActionLevel = 1;

    /// <summary>
    /// The highest level this action can become.
    /// </summary>
    public int MaxLevel = 5;

    /// <summary>
    /// Blood cost deducted from the vampire's bloodstream to activate.
    /// </summary>
    [DataField]
    public float BloodCost = 0f;

    /// <summary>
    /// Humanity lost when activating this action.
    /// </summary>
    [DataField]
    public float HumanityCost = 0f;

    /// <summary>
    /// If true, this action cannot be started while the vampire is in frenzy.
    /// </summary>
    [DataField]
    public bool DisabledInFrenzy = false;

    #endregion
}
