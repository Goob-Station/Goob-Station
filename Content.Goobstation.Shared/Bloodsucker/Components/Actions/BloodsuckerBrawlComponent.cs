using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

/// <summary>
/// Configuration for the brawl action.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerBrawlComponent : Component
{
    /// <summary>
    /// How long to knock someone out for if applicable.
    /// </summary>
    [DataField]
    public float KnockoutTime = 2f;

    /// <summary>
    /// EMP range, if applicable.
    /// </summary>
    public float EMPRadius = 2f;

    /// <summary>
    /// EMP energy consumption, if applicable.
    /// </summary>
    public float EMPConsumption = 500f;

    /// <summary>
    /// EMP duration, if applicable.
    /// </summary>
    public float EMPDuration = 30f;

    /// <summary>
    /// The sound that plays once the doafter completes.
    /// </summary>
    [DataField]
    public SoundSpecifier? UseSound = new SoundPathSpecifier("/Audio/Weapons/Guns/Gunshots/bang.ogg");

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
