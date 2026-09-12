using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

/// <summary>
/// Configuration for the predatory lunge action.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerMesmerizeComponent : Component
{
    /// <summary>
    /// Initial do-after delay before mesmerizing.
    /// </summary>
    [DataField]
    public float StartDelay = 5f;

    /// <summary>
    /// How long to paralyze the target for, in seconds. Scales with level: base + level * 15.
    /// </summary>
    [DataField]
    public float ParalyzeBase = 90f;

    [DataField]
    public float ParalyzePerLevel = 15f;

    /// <summary>
    /// Range within which mesmerize can be used.
    /// </summary>
    [DataField]
    public float Range = 8f;

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/hiero_blast.ogg");

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
