using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Forcefully grabs a mob or forces a door open.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlasherRelentlessGrabComponent : Component
{
    /// <summary>
    /// The action entity.
    /// </summary>
    [DataField]
    public EntProtoId ActionId = "ActionSlasherRelentlessGrab";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEnt;

    [DataField]
    public TimeSpan DoorDisableDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Fear needed for the grab to be a hard grab instead of a soft one.
    /// </summary>
    [DataField]
    public float HardGrabFear = 20f;

    /// <summary>
    /// Fear needed for the grab to also stun the victim.
    /// </summary>
    [DataField]
    public float StunGrabFear = 41f;

    [DataField]
    public TimeSpan GrabStunDuration = TimeSpan.FromSeconds(1.5);

    [DataField]
    public EntProtoId? DoorJackEffect = "EffectSparks";
}
