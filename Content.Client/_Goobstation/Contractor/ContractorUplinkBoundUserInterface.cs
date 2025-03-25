using Content.Shared._Goobstation.Contracts;
using Content.Shared._Goobstation.Contracts.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Goobstation.Contractor
{
    [UsedImplicitly]
    public sealed class ContractorUplinkBoundUserInterface : BoundUserInterface
    {
        private ContractorUplink? _uplink;

        public ContractorUplinkBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {

        }

        protected override void Open()
        {
            base.Open();

            _uplink = this.CreateWindow<ContractorUplink>();
            _uplink.OnContractButtonClicked += ButtonPressed;
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            var castState = (ContractorUplinkBoundUserInterfaceState) state;
            _uplink?.UpdateState(castState); //Update window state
        }

        private void ButtonPressed(ContractorUiMessage message)
        {
            SendMessage(message);
        }
    }
}
