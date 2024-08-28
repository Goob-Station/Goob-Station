using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Hailer.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HailerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId HailerAction = "ActionHailer";

    [DataField, AutoNetworkedField]
    public EntityUid? HailActionEntity;
}
