using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cyberdeck.Components;

[RegisterComponent]
public sealed partial class CyberdeckProjectionComponent : Component
{
    [ViewVariables]
    public EntityUid? ReturnAction;

    [DataField]
    public EntProtoId ReturnActionId = "ActionCyberdeckVisionReturn";
}
