using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.ShadowDemon;

/// <summary>
/// Grants you shadow grapple action
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ShadowGrappleComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ShadowGrappleAction";

    [ViewVariables, AutoNetworkedField]
    public EntityUid? ActionUid;
}
