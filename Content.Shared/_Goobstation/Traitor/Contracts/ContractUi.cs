using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Traitor.Contracts;

[Serializable, NetSerializable]
public enum ContractUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class ContractMenuState : BoundUserInterfaceState
{
    public List<ContractData> AvailableContracts { get; }

    public List<ContractData> ActiveContracts { get; }

    public List<ContractData> CompletedContracts { get; }

    public int TotalEarned { get; }

    public int MaxActiveContracts { get; }

    public ContractMenuState(
        List<ContractData> availableContracts,
        List<ContractData> activeContracts,
        List<ContractData> completedContracts,
        int totalEarned,
        int maxActiveContracts)
    {
        AvailableContracts = availableContracts;
        ActiveContracts = activeContracts;
        CompletedContracts = completedContracts;
        TotalEarned = totalEarned;
        MaxActiveContracts = maxActiveContracts;
    }
}

[Serializable, NetSerializable]
public sealed class ContractAcceptMessage : BoundUserInterfaceMessage
{
    public ProtoId<ContractPrototype> ContractId { get; }

    public ContractAcceptMessage(ProtoId<ContractPrototype> contractId)
    {
        ContractId = contractId;
    }
}

[Serializable, NetSerializable]
public sealed class ContractAbandonMessage : BoundUserInterfaceMessage
{
    public int ContractInstanceId { get; }

    public ContractAbandonMessage(int contractInstanceId)
    {
        ContractInstanceId = contractInstanceId;
    }
}

[Serializable, NetSerializable]
public sealed class ContractRequestUpdateMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class ContractClaimRewardMessage : BoundUserInterfaceMessage
{
    public int ContractInstanceId { get; }

    public ContractClaimRewardMessage(int contractInstanceId)
    {
        ContractInstanceId = contractInstanceId;
    }
}
