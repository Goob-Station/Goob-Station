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
    public NetEntity CurrentExtractionPoint = NetEntity.Invalid;

    [DataField]
    [AutoNetworkedField]
    public int Tc;

    [DataField]
    [AutoNetworkedField]
    public int TotalTc;

    [DataField]
    [AutoNetworkedField]
    public int Rep;
}

[RegisterComponent]
public sealed partial class ContractorMarkerComponent : Component
{
    [DataField]
    public LocId? Name;
}

[Serializable, NetSerializable]
public sealed class ContractorUplinkBoundUserInterfaceState(
    int tc,
    Dictionary<NetEntity, List<NetEntity>> contracts,
    int rep,
    NetEntity currentTarget,
    NetEntity currentExtractionPoint) : BoundUserInterfaceState
{
    public readonly int Tc = tc;
    public readonly int Rep = rep;
    public readonly NetEntity CurrentTarget = currentTarget;
    public readonly NetEntity CurrentExtractionPoint = currentExtractionPoint;
    public readonly Dictionary<NetEntity, List<NetEntity>> Contracts = contracts;
}

[Serializable, NetSerializable]
public enum ContractorUplinkUiKey
{
    Key,
}

[Serializable, NetSerializable]
public enum UiMessage
{
    SelectTarget,
    Refresh,
}

[Serializable, NetSerializable]
public sealed class ContractorUiMessage(UiMessage button) : BoundUserInterfaceMessage
{
    public readonly UiMessage Button = button;
}

[RegisterComponent, NetworkedComponent ,AutoGenerateComponentState]
public sealed partial class ContractorUplinkComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Used;
}
