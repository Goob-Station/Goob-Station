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

    [DataField]
    public EntProtoId? DoorJackEffect = "EffectSparks";
}
