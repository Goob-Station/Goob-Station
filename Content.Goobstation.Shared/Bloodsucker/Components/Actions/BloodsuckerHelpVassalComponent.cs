using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Vassals;

/// <summary>
/// Configuration for the Help Vassal action.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerHelpVassalComponent : Component
{
    /// <summary>
    /// Do-after delay when bringing an ex-vassal back into the fold.
    /// </summary>
    [DataField]
    public float ReturnDelay = 5f;

    /// <summary>
    /// How much blood the vampire loses when converting a blood bag.
    /// </summary>
    [DataField]
    public float BloodBagBloodCost = 150f;

    /// <summary>
    /// The entity prototype to spawn as the converted vampire blood bag.
    /// </summary>
    [DataField]
    public EntProtoId BloodBagProto = "Bloodpack";

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Lavaland/heartbeat.ogg");

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

