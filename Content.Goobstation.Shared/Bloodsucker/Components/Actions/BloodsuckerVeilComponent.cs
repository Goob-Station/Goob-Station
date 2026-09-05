using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Preferences;
using Robust.Shared.Audio;
using Robust.Shared.Enums;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodsuckerVeilComponent : Component
{
    /// <summary>
    /// Is the veil currently active?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Active;

    [DataField]
    public string? SavedName;

    [DataField]
    public Sex SavedSex;

    [DataField]
    public Gender SavedGender;

    [DataField]
    public int SavedAge;

    [DataField]
    public Color SavedSkinColor;

    [DataField]
    public Color SavedEyeColor;

    [DataField]
    public List<Marking>? SavedMarkings;

    [DataField]
    public float SavedHeight;

    [DataField]
    public float SavedWidth;

    /// <summary>
    /// Blood drained per second while veil is active.
    /// </summary>
    [DataField]
    public float BloodDrainPerSecond = 0.1f;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan UpdateTimer;

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/smoke.ogg");

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
