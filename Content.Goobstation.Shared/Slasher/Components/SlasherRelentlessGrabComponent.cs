using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// When activated, the slasher's next melee hit will hard grab the target.
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

    /// <summary>
    /// Whether the grab is ready.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Ready = false;
}
