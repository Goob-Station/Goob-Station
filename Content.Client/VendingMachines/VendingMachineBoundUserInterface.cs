// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.UserInterface.Controls;
using Content.Client.VendingMachines.UI;
using Content.Shared.VendingMachines;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using System.Linq;

namespace Content.Client.VendingMachines
{
    public sealed class VendingMachineBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private VendingMachineMenu? _menu;

        [ViewVariables]
        private List<VendingMachineInventoryEntry> _cachedInventory = new();

        // Pirate banking start
        [ViewVariables]
        private double _priceMultiplier;
        [ViewVariables]
        private int _credits;
        // Pirate banking end

        public VendingMachineBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _menu = this.CreateWindowCenteredLeft<VendingMachineMenu>();
            _menu.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
            _menu.OnItemSelected += OnItemSelected;
            _menu.OnWithdraw += OnWithdrawPressed; // Pirate banking
            //Refresh();
        }

        // Pirate banking start
        private void OnWithdrawPressed(VendingMachineWithdrawMessage message)
        {
            SendPredictedMessage(new VendingMachineWithdrawMessage());
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not VendingMachineInterfaceState newState)
                return;

            _cachedInventory = newState.Inventory;
            _priceMultiplier = newState.PriceMultiplier;
            _credits = newState.Credits;
            
            Refresh();
        }
        // Pirate banking end

        public void Refresh()
        {
            var enabled = EntMan.TryGetComponent(Owner, out VendingMachineComponent? bendy) && !bendy.Ejecting;

            // Pirate banking
            // var system = EntMan.System<VendingMachineSystem>();
            // _cachedInventory = system.GetAllInventory(Owner);

            _menu?.Populate(_cachedInventory, enabled, _priceMultiplier, _credits); // Pirate banking
        }

        public void UpdateAmounts()
        {
            var enabled = EntMan.TryGetComponent(Owner, out VendingMachineComponent? bendy) && !bendy.Ejecting;

            // Pirate banking
            // var system = EntMan.System<VendingMachineSystem>();
            // _cachedInventory = system.GetAllInventory(Owner);
            _menu?.UpdateAmounts(_cachedInventory, enabled, _priceMultiplier, _credits); // Pirate banking
        }

        private void OnItemSelected(GUIBoundKeyEventArgs args, ListData data)
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            if (data is not VendorItemsListData { ItemIndex: var itemIndex })
                return;

            if (_cachedInventory.Count == 0)
                return;

            var selectedItem = _cachedInventory.ElementAtOrDefault(itemIndex);

            if (selectedItem == null)
                return;

            SendPredictedMessage(new VendingMachineEjectMessage(selectedItem.Type, selectedItem.ID));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            if (_menu == null)
                return;

            _menu.OnItemSelected -= OnItemSelected;
            _menu.OnWithdraw -= OnWithdrawPressed; // Pirate banking
            _menu.OnClose -= Close;
            _menu.Dispose();
        }
    }
}