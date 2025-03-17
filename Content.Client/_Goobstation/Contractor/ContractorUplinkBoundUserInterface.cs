using Content.Shared._Goobstation.Contracts;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Goobstation.Contractor
{
    [UsedImplicitly]
    public sealed class ContractorUplinkBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
    {
        private ContractorUplink? _uplink;

        protected override void Open()
        {
            base.Open();

            SendMessage(new ContractorUiMessage(UiMessage.Refresh));
            _uplink = this.CreateWindow<ContractorUplink>();
            _uplink.OnContractButtonClicked += ButtonPressed;
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            var castState = (ContractorUplinkBoundUserInterfaceState) state;
            _uplink?.UpdateState(castState); //Update window state
        }

        public void ButtonPressed(UiMessage message)
        {
            SendMessage(new ContractorUiMessage(message));
        }
    }
}
