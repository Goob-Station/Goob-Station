using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

/// <summary>
/// Configuration for the Cloak action.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class BloodsuckerCloakComponent : Component
{
    /// <summary>
    /// Is the cloak currently active?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Active;

    /// <summary>
    /// Whether the vampire was running before cloaking, so we can restore their intent on deactivation.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool WasRunning;

    /// <summary>
    /// True if the entity already had LightDetectionComponent before we added it.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool HadLightDetection;

    /// <summary>
    /// Visibility at level 1. Gets lower (more invisible) with each level.
    /// </summary>
    [DataField]
    public float BaseVisibility = 0.7f;

    /// <summary>
    /// Visibility reduction per level.
    /// </summary>
    [DataField]
    public float VisibilityPerLevel = 0.1f;

    /// <summary>
    /// Minimum visibility regardless of level.
    /// </summary>
    [DataField]
    public float MinVisibility = 0.1f;

    /// <summary>
    /// How far away (in tiles) to check for witnesses before allowing activation.
    /// </summary>
    [DataField]
    public float WitnessRange = 9f;

    /// <summary>
    /// Blood drained per second while cloaked.
    /// </summary>
    [DataField]
    public float BloodDrainPerSecond = 0.2f;

    /// <summary>
    /// Damage dealt when the vampire tries to run while cloaked.
    /// </summary>
    [DataField]
    public float RunPenaltyDamage = 10f;

    [DataField]
    public SoundSpecifier? ActivateSound = new SoundPathSpecifier("/Audio/Effects/toss.ogg");

    [DataField]
    public SoundSpecifier? DeactivateSound = new SoundPathSpecifier("/Audio/Effects/stealthoff.ogg");

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan UpdateTimer;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

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
