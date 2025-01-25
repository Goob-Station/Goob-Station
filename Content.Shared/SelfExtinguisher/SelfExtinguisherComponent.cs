using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.SelfExtinguisher;

/// <summary>
///   When equipped, the SelfExtinguisherComponent will give an action to its wearer to self-extinguish when set on fire.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class SelfExtinguisherComponent : Component
{
    /// <summary>
    ///   Action used to self-extinguish.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId Action = "ActionSelfExtinguish";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    /// <summary>
    ///   Cooldown before the self-extinguisher can be used again.
    /// </summary>
    [DataField(required: true)]
    public TimeSpan Cooldown;

    /// <summary>
    ///   Time before the self-extinguisher can be used again.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextExtinguish = TimeSpan.Zero;

    /// <summary>
    ///   When failing self-extinguish attempts,
    ///   don't spam popups every frame and instead have a cooldown.
    /// </summary>
    [DataField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(1);

    /// <summary>
    ///   Time before the next popup can be shown.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan? NextPopup = null;

    /// <summary>
    ///   If true, requires the wearer to be immune to gas ignition.
    /// </summary>
    [DataField]
    public bool RequiresIgniteFromGasImmune = false;

    /// <summary>
    ///   The sound effect that plays upon an extinguish.
    /// </summary>
    [DataField(required: true)]
    public SoundSpecifier Sound { get; private set; } = default!;

    /// <summary>
    ///   The sound effect that plays upon getting refilled.
    /// </summary>
    [DataField]
    public SoundSpecifier RefillSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagIn/revolver_magin.ogg");
}
