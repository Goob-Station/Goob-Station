using Content.Shared._Goobstation.Traitor.Contracts;
using Robust.Client.UserInterface;

namespace Content.Client._Goobstation.Traitor.Contracts;

public sealed class ContractBoundUserInterface : BoundUserInterface
{
    private ContractMenu? _menu;

    public ContractBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ContractMenu>();
        _menu.OnContractAccept += OnAccept;
        _menu.OnContractAbandon += OnAbandon;
        _menu.OnContractClaimReward += OnClaimReward;

        SendMessage(new ContractRequestUpdateMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is ContractMenuState contractState)
        {
            _menu?.UpdateState(contractState);
        }
    }

    private void OnAccept(Robust.Shared.Prototypes.ProtoId<ContractPrototype> contractId)
    {
        SendMessage(new ContractAcceptMessage(contractId));
    }

    private void OnAbandon(int contractInstanceId)
    {
        SendMessage(new ContractAbandonMessage(contractInstanceId));
    }

    private void OnClaimReward(int contractInstanceId)
    {
        SendMessage(new ContractClaimRewardMessage(contractInstanceId));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _menu?.Dispose();
        }
    }
}
