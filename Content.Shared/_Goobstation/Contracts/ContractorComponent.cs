using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Contracts;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ContractorComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    [AutoNetworkedField]

    public Dictionary<NetEntity, List<NetEntity>> Contracts = new(5);

    [DataField]
    [AutoNetworkedField]
    public NetEntity CurrentTarget = NetEntity.Invalid;

    [DataField]
    [AutoNetworkedField]
    public int Rep = 0;
}

[RegisterComponent]
public sealed partial class ContractorMarkerComponent : Component // this is stupid but station beacons can be moved so i need this i thinmk
{
    [DataField]
    public LocId? Name;
}

[Serializable, NetSerializable]
public sealed class ContractorUplinkBoundUserInterfaceState(int tc, List<NetEntity> contracts) : BoundUserInterfaceState
{
    public readonly int Tc = tc;
    public readonly List<NetEntity> Contracts = contracts;
}


[Serializable, NetSerializable]
public enum ContractorUplinkUiKey
{
    Key
}

