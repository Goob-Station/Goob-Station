// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Guidebook.Components;
using Content.Client.UserInterface.Controls;
using Content.Goobstation.Shared.Chemistry;
using Content.Shared.Containers.ItemSlots;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Chemistry.UI
{
    /// <summary>
    /// Initializes a <see cref="EnergyReagentDispenserWindow"/> and updates it when new server messages are received.
    /// </summary>
    [UsedImplicitly]
    public sealed class EnergyReagentDispenserBoundUserInterface(EntityUid owner, Enum uiKey)
        : BoundUserInterface(owner, uiKey)
    {
        [ViewVariables]
        private EnergyReagentDispenserWindow? _window;

        /// <summary>
        /// Called each time a dispenser UI instance is opened. Generates the dispenser window and fills it with
        /// relevant info. Sets the actions for static buttons.
        /// <para>Buttons which can change like reagent dispense buttons have their actions set in <see cref="UpdateReagentsList"/>.</para>
        /// </summary>
        protected override void Open()
        {
            base.Open();

            // Setup window layout/elements
            _window = this.CreateWindow<EnergyReagentDispenserWindow>();
            _window.SetInfoFromEntity(EntMan, Owner);

            // Setup static button actions.
            _window.EjectButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent(SharedEnergyReagentDispenser.OutputSlotName));
            _window.ClearButton.OnPressed += _ => SendMessage(new EnergyReagentDispenserClearContainerSolutionMessage());

            _window.AmountGrid.OnButtonPressed += s => SendMessage(new EnergyReagentDispenserSetDispenseAmountMessage(s));

            _window.OnDispenseReagentButtonPressed += (reagentId) => SendMessage(new EnergyReagentDispenserDispenseReagentMessage(reagentId));
        }

        /// <summary>
        /// Update the UI each time new state data is sent from the server.
        /// </summary>
        protected override void UpdateState(BoundUserInterfaceState message)
        {
            base.UpdateState(message);

            if (message is not EnergyReagentDispenserBoundUserInterfaceState state)
                return;

            _window?.UpdateState(state);
        }
    }
}
