using Content.Shared.Nyanotrasen.Kitchen.UI;
using Robust.Client.GameObjects;

namespace Content.Client.Nyanotrasen.Kitchen.UI
{
    public sealed class DeepFryerBoundUserInterface : BoundUserInterface
    {
        private DeepFryerWindow? _window;

        private NetEntity[] _entities = default!;

        public DeepFryerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
            base.Open();
            _window = new DeepFryerWindow();
            _window.OnClose += Close;
            _window.ItemList.OnItemSelected += args =>
            {
                SendMessage(new DeepFryerRemoveItemMessage(_entities[args.ItemIndex]));
            };
            _window.InsertItem.OnPressed += _ =>
            {
                SendMessage(new DeepFryerInsertItemMessage());
            };
            _window.ScoopVat.OnPressed += _ =>
            {
                SendMessage(new DeepFryerScoopVatMessage());
            };
            _window.ClearSlag.OnPressed += args =>
            {
                SendMessage(new DeepFryerClearSlagMessage());
            };
            _window.RemoveAllItems.OnPressed += _ =>
            {
                SendMessage(new DeepFryerRemoveAllItemsMessage());
            };
            _window.OpenCentered();
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (_window == null)
                return;

            if (state is not DeepFryerBoundUserInterfaceState cast)
                return;

            _entities = cast.ContainedEntities;
            _window.UpdateState(cast);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
                return;

            _window?.Dispose();
        }
    }
}
