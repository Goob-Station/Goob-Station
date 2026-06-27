using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AugmentedEyesightComponent : Component
{
    /// <summary>
    /// Enabled = Flash protection,
    /// Disabled = X-ray and flash vulnerability
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled;

    [DataField, AutoNetworkedField]
    public EntProtoId ActionId = "ActionAugmentedEyesight";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEnt;

    /// <summary>
    /// Used for GetEyeProtectionEvent
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan EyeProtectionTime = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Multiplier applied to flash durations while active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FlashMultiplier = 2.0f;
}
