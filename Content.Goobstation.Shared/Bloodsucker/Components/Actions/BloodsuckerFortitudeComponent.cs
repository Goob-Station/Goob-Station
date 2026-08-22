using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

/// <summary>
/// Configuration for the Fortitude action.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class BloodsuckerFortitudeComponent : Component
{
    /// <summary>
    /// The DamageModifierSetId the entity had before fortitude was activated, so we can restore it.
    /// </summary>
    [DataField]
    public string? PreviousModifierSetId;

    /// <summary>
    /// Modifier set prototype IDs indexed by level (1-5).
    /// </summary>
    [DataField]
    public List<string> FortitudeModifierSetIds = new()
{
    "BloodsuckerFortitude1",
    "BloodsuckerFortitude2",
    "BloodsuckerFortitude3",
    "BloodsuckerFortitude4",
    "BloodsuckerFortitude5",
};

    /// <summary>
    /// Is fortitude currently active?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Active;

    /// <summary>
    /// Minimum level required to gain stun immunity.
    /// </summary>
    [DataField]
    public int StunImmuneLevel = 2;

    /// <summary>
    /// Damage dealt when the vampire tries to run while fortitude is active.
    /// </summary>
    [DataField]
    public float RunPenaltyDamage = 10f;

    /// <summary>
    /// Blood drained per second while fortitude is active.
    /// </summary>
    [DataField]
    public float BloodDrainPerSecond = 0.2f;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan UpdateTimer;

    [DataField]
    public SoundSpecifier? ActivateSound = new SoundPathSpecifier("/Audio/_Goobstation/Bloodsucker/Effects/heavy1.ogg");

    [DataField]
    public SoundSpecifier? DeactivateSound = new SoundPathSpecifier("/Audio/_Goobstation/Bloodsucker/Effects/heavy2.ogg");

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
