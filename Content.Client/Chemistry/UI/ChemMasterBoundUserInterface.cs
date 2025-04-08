// SPDX-FileCopyrightText: 2020 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Spartak <artak10t@gmail.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2022 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Illiux <newoutlook@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Dora <27211909+catdotjs@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry;
using Content.Shared.Containers.ItemSlots;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Chemistry.UI
{
    /// <summary>
    /// Initializes a <see cref="ChemMasterWindow"/> and updates it when new server messages are received.
    /// </summary>
    [UsedImplicitly]
    public sealed class ChemMasterBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private ChemMasterWindow? _window;

        public ChemMasterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        /// <summary>
        /// Called each time a chem master UI instance is opened. Generates the window and fills it with
        /// relevant info. Sets the actions for static buttons.
        /// </summary>
        protected override void Open()
        {
            base.Open();

            // Setup window layout/elements
            _window = this.CreateWindow<ChemMasterWindow>();
            _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;

            // Setup static button actions.
            _window.InputEjectButton.OnPressed += _ => SendMessage(
                new ItemSlotButtonPressedEvent(SharedChemMaster.InputSlotName));
            _window.OutputEjectButton.OnPressed += _ => SendMessage(
                new ItemSlotButtonPressedEvent(SharedChemMaster.OutputSlotName));
            _window.BufferTransferButton.OnPressed += _ => SendMessage(
                new ChemMasterSetModeMessage(ChemMasterMode.Transfer));
            _window.BufferDiscardButton.OnPressed += _ => SendMessage(
                new ChemMasterSetModeMessage(ChemMasterMode.Discard));
            _window.CreatePillButton.OnPressed += _ => SendMessage(
                new ChemMasterCreatePillsMessage(
                    (uint) _window.PillDosage.Value, (uint) _window.PillNumber.Value, _window.LabelLine));
            _window.CreateBottleButton.OnPressed += _ => SendMessage(
                new ChemMasterOutputToBottleMessage(
                    (uint) _window.BottleDosage.Value, _window.LabelLine));
            _window.BufferSortButton.OnPressed += _ => SendMessage(
                    new ChemMasterSortingTypeCycleMessage());

            for (uint i = 0; i < _window.PillTypeButtons.Length; i++)
            {
                var pillType = i;
                _window.PillTypeButtons[i].OnPressed += _ => SendMessage(new ChemMasterSetPillTypeMessage(pillType));
            }

            _window.OnReagentButtonPressed += (args, button) => SendMessage(new ChemMasterReagentAmountButtonMessage(button.Id, button.Amount, button.IsBuffer));
        }

        /// <summary>
        /// Update the ui each time new state data is sent from the server.
        /// </summary>
        /// <param name="state">
        /// Data of the <see cref="SharedReagentDispenserComponent"/> that this ui represents.
        /// Sent from the server.
        /// </param>
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            var castState = (ChemMasterBoundUserInterfaceState) state;

            _window?.UpdateState(castState); // Update window state
        }
    }
}