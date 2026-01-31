using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Movement;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CursorOffsetActionComponent : Component
{
    [DataField]
    public EntProtoId CursorOffsetActionId = "ActionEyeZoom";

    [DataField]
    public EntityUid? CursorOffsetActionEntity;

    [DataField, AutoNetworkedField]
    public bool Active = false;
}
