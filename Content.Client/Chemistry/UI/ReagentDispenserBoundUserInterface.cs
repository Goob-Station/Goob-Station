// SPDX-FileCopyrightText: 2019 Remie Richards <remierichards@gmail.com>
// SPDX-FileCopyrightText: 2019 moneyl <8206401+Moneyl@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2022 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 AWF <you@example.com>
// SPDX-FileCopyrightText: 2024 Brandon Li <48413902+aspiringLich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 GitHubUser53123 <110841413+GitHubUser53123@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2024 Kira Bridgeton <161087999+Verbalase@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 ike709 <ike709@github.com>
// SPDX-FileCopyrightText: 2024 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Guidebook.Components;
using Content.Client.UserInterface.Controls;
using Content.Shared.Chemistry;
using Content.Shared.Containers.ItemSlots;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Chemistry.UI
{
    /// <summary>
    /// Initializes a <see cref="ReagentDispenserWindow"/> and updates it when new server messages are received.
    /// </summary>
    [UsedImplicitly]
    public sealed class ReagentDispenserBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private ReagentDispenserWindow? _window;

        public ReagentDispenserBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        /// <summary>
        /// Called each time a dispenser UI instance is opened. Generates the dispenser window and fills it with
        /// relevant info. Sets the actions for static buttons.
        /// <para>Buttons which can change like reagent dispense buttons have their actions set in <see cref="UpdateReagentsList"/>.</para>
        /// </summary>
        protected override void Open()
        {
            base.Open();

            // Setup window layout/elements
            _window = this.CreateWindow<ReagentDispenserWindow>();
            _window.SetInfoFromEntity(EntMan, Owner);

            // Setup static button actions.
            _window.EjectButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent(SharedReagentDispenser.OutputSlotName));
            _window.ClearButton.OnPressed += _ => SendMessage(new ReagentDispenserClearContainerSolutionMessage());

            _window.AmountGrid.OnButtonPressed += s => SendMessage(new ReagentDispenserSetDispenseAmountMessage(s));

            _window.OnDispenseReagentButtonPressed += (location) => SendMessage(new ReagentDispenserDispenseReagentMessage(location));
            _window.OnEjectJugButtonPressed += (location) => SendMessage(new ReagentDispenserEjectContainerMessage(location));
            _window.OnToggleValveButtonPressed += () => SendMessage(new ReagentDispenserToggleValveMessage()); // Pirate: chem plumbing
            BindPirateRecipeActions(); // Pirate: chem recipes
        }

        /// <summary>
        /// Update the UI each time new state data is sent from the server.
        /// </summary>
        /// <param name="state">
        /// Data of the <see cref="ReagentDispenserComponent"/> that this UI represents.
        /// Sent from the server.
        /// </param>
        #region Pirate: chem recipes
        private void BindPirateRecipeActions()
        {
            if (_window == null)
                return;

            _window.OnStartRecipeRecordingPressed += () => SendMessage(new ReagentDispenserStartRecipeRecordingMessage());
            _window.OnCancelRecipeRecordingPressed += () => SendMessage(new ReagentDispenserCancelRecipeRecordingMessage());
            _window.OnSaveRecipePressed += name => SendMessage(new ReagentDispenserSaveRecipeMessage(name));
            _window.OnClearRecipesPressed += () => SendMessage(new ReagentDispenserClearRecipesMessage());
            _window.OnDispenseRecipePressed += name => SendMessage(new ReagentDispenserDispenseRecipeMessage(name));
            _window.OnDeleteRecipePressed += name => SendMessage(new ReagentDispenserDeleteRecipeMessage(name));
            _window.OnSaveRecipeToDiskPressed += name => SendMessage(new ReagentDispenserSaveRecipeToDiskMessage(name));
            _window.OnCopyDiskRecipePressed += name => SendMessage(new ReagentDispenserCopyDiskRecipeMessage(name));
            _window.OnDispenseDiskRecipePressed += name => SendMessage(new ReagentDispenserDispenseDiskRecipeMessage(name));
            _window.OnDeleteDiskRecipePressed += name => SendMessage(new ReagentDispenserDeleteDiskRecipeMessage(name));
            _window.OnEjectRecipeDiskPressed += () => SendMessage(new ItemSlotButtonPressedEvent(SharedReagentDispenser.RecipeDiskSlotName));
        }
        #endregion
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            var castState = (ReagentDispenserBoundUserInterfaceState) state;
            _window?.UpdateState(castState); //Update window state
        }
    }
}