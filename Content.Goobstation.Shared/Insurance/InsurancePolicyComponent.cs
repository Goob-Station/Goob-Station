using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Insurance;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InsurancePolicyComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityWhitelist ValidEntities;

    [DataField]
    public List<EntProtoId>? ExtraCompensationItems;
}
