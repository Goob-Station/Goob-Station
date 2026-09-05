using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

/// <summary>
/// Configuration for the Feed action.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerFeedComponent : Component
{
    /// <summary>
    /// Blood volume transferred from target to vampire per drain tick.
    /// </summary>
    [DataField]
    public float BloodDrainAmount = 10f;

    /// <summary>
    /// Initial do-after delay before the first drain tick fires.
    /// </summary>
    [DataField]
    public float StartDelay = 4f;

    /// <summary>
    /// How long it takes inbetween each sip.
    /// </summary>
    [DataField]
    public float SipDelay = 2f;

    /// <summary>
    /// How long the target sleeps when fed upon while held in an aggressive grab.
    /// </summary>
    [DataField]
    public float SleepDuration = 60f;

    /// <summary>
    /// The sound that plays once the doafter completes.
    /// </summary>
    [DataField]
    public SoundSpecifier? DrinkSound = new SoundPathSpecifier("/Audio/Items/drink.ogg");

    /// <summary>
    /// The range at which your feeding is noticed by others.
    /// </summary>
    [DataField]
    public float FeedNoticeRange = 2f;

    /// <summary>
    /// The first warning as to the level of the target's blood level.
    /// </summary>
    [DataField]
    public float BloodWarningSafe = 0.75f;

    /// <summary>
    /// The second warning as to the level of the target's blood level.
    /// </summary>
    [DataField]
    public float BloodWarningDanger = 0.50f;

    /// <summary>
    /// <summary>
    /// The final warning as to the level of the target's blood level.
    /// </summary>
    [DataField]
    public float BloodWarningFatal = 0.25f;

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
