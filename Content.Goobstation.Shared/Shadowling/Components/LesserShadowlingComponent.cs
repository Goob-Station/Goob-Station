using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components;

/// <summary>
/// This is used for indicating that the user is Lesser Shadowling
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LesserShadowlingComponent : Component
{
    [DataField]
    public EntProtoId ShadowWalkActionId = "ActionShadowWalk";

    [ViewVariables]
    public EntityUid? ShadowWalkAction;

    [DataField]
    public ProtoId<AlertPrototype> AlertProto = "ShadowlingLight";
}
