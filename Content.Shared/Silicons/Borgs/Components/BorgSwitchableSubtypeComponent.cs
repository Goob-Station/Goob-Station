using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Silicons.Borgs.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class BorgSwitchableSubtypeComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<BorgSubtypePrototype>? BorgSubtype;
}
