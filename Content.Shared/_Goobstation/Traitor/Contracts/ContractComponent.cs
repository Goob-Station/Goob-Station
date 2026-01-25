using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Traitor.Contracts;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ContractsComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ContractData> ActiveContracts { get; set; } = new();

    [DataField, AutoNetworkedField]
    public List<ContractData> CompletedContracts { get; set; } = new();

    [DataField, AutoNetworkedField]
    public List<ContractData> FailedContracts { get; set; } = new();

    [DataField, AutoNetworkedField]
    public int TotalEarned { get; set; } = 0;

    [DataField]
    public int MaxActiveContracts { get; set; } = 3;

    [DataField]
    public EntityUid? LinkedUplink { get; set; }
}

[Serializable, NetSerializable]
public sealed class ContractData
{
    public int ContractId { get; set; }

    public ProtoId<ContractPrototype> PrototypeId { get; set; } = string.Empty;

    public NetEntity? ObjectiveEntity { get; set; }

    public float Progress { get; set; } = 0f;

    public TimeSpan AcceptedTime { get; set; }

    public TimeSpan? CompletedTime { get; set; }

    public ContractStatus Status { get; set; } = ContractStatus.Available;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Reward { get; set; }

    public string? TargetName { get; set; }
}

[Serializable, NetSerializable]
public enum ContractStatus : byte
{
    Available,
    Active,
    Completed,
    Failed,
    Abandoned
}
