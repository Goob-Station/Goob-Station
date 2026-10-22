using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MalfAI;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MalfAiComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? StoreAction;

    [DataField]
    public EntProtoId StoreActionId = "ActionMalfAiOpenStore";
}
