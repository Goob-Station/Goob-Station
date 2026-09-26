using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

/// <summary>
/// Configuration for the predatory lunge action.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerLungeComponent : Component
{

    public EntityUid CurrentTarget = EntityUid.Invalid;

    /// <summary>
    /// Do-after wind-up delay before the lunge (waived at level 4+).
    /// </summary>
    [DataField]
    public float StartDelay = 2f;

    /// <summary>
    /// Speed at which the vampire is thrown toward the target.
    /// </summary>
    [DataField]
    public float DashSpeed = 18f;

    /// <summary>
    /// Base knockdown duration when hitting from behind/darkness, in seconds.
    /// Scales with level: base + level * KnockdownPerLevel.
    /// </summary>
    [DataField]
    public float KnockdownBase = 1f;

    [DataField]
    public float KnockdownPerLevel = 0.5f;

    [DataField]
    public SoundSpecifier? LungeSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");

    /// <summary>
    /// True while the vampire is mid-lunge, used to gate the collision handler.
    /// </summary>
    [DataField]
    public bool IsLeaping;

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
