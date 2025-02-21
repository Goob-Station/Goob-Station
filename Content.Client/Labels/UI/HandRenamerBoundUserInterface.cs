using Content.Client.Labels.RenameSystem;
using Content.Shared.HandRenamer;

namespace Content.Client.Labels.UI
{
    public sealed class HandRenamerBoundUserInterface : HandLabelerBoundUserInterface
    {
        public HandRenamerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {

        }

        protected override void OnLabelChanged(string newLabel)
        {
            if (_entManager.TryGetComponent(Owner, out HandRenamerComponent? component) &&
                component.AssignedName.Equals(newLabel))
                return;

            SendPredictedMessage(new HandRenamerNameChangedMessage(newLabel));
        }
    }
}
