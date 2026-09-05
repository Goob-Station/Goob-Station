using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class BloodsuckerMasqueradeComponent : Component
{
    /// <summary>
    /// Is masquerade currently active?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Active;

    /// <summary>
    /// Blood drained per second while active.
    /// </summary>
    [DataField]
    public float BloodDrainPerSecond = 0.1f;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan UpdateTimer;

    [DataField]
    public SoundSpecifier? ActivateSound =
        new SoundPathSpecifier("/Audio/_Lavaland/heartbeat.ogg");

    [DataField]
    public SoundSpecifier? DeactivateSound =
        new SoundPathSpecifier("/Audio/_Lavaland/heartbeat.ogg");

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
    public float BloodCost = 10f;

    /// <summary>
    /// Humanity lost when activating this action.
    /// </summary>
    [DataField]
    public float HumanityCost = 0f;

    /// <summary>
    /// If true, this action cannot be started while the vampire is in frenzy.
    /// </summary>
    [DataField]
    public bool DisabledInFrenzy = true;

    #endregion
}
