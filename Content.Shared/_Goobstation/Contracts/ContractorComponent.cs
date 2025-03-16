using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Contracts;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ContractorComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    [AutoNetworkedField]
    public List<NetEntity> Contracts = [];

    [DataField]
    [AutoNetworkedField]
    public NetEntity CurrentTarget = NetEntity.Invalid;
}
