// SPDX-FileCopyrightText: 2020 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2020 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Disposal;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controls;
using static Content.Shared.Disposal.Components.SharedDisposalUnitComponent;

namespace Content.Client.Disposal.UI
{
    /// <summary>
    /// Initializes a <see cref="MailingUnitWindow"/> or a <see cref="DisposalUnitWindow"/> and updates it when new server messages are received.
    /// </summary>
    [UsedImplicitly]
    public sealed class DisposalUnitBoundUserInterface : BoundUserInterface
    {
        // What are you doing here
        [ViewVariables]
        public MailingUnitWindow? MailingUnitWindow;

        [ViewVariables]
        public DisposalUnitWindow? DisposalUnitWindow;

        public DisposalUnitBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        private void ButtonPressed(UiButton button)
        {
            SendMessage(new UiButtonPressedMessage(button));
            // If we get client-side power stuff then we can predict the button presses but for now we won't as it stuffs
            // the pressure lerp up.
        }

        private void TargetSelected(ItemList.ItemListSelectedEventArgs args)
        {
            var item = args.ItemList[args.ItemIndex];
            SendMessage(new TargetSelectedMessage(item.Text));
        }

        protected override void Open()
        {
            base.Open();

            if (UiKey is MailingUnitUiKey)
            {
                MailingUnitWindow = new MailingUnitWindow();

                MailingUnitWindow.OpenCenteredRight();
                MailingUnitWindow.OnClose += Close;

                MailingUnitWindow.Eject.OnPressed += _ => ButtonPressed(UiButton.Eject);
                MailingUnitWindow.Engage.OnPressed += _ => ButtonPressed(UiButton.Engage);
                MailingUnitWindow.Power.OnPressed += _ => ButtonPressed(UiButton.Power);

                MailingUnitWindow.TargetListContainer.OnItemSelected += TargetSelected;
            }
            else if (UiKey is DisposalUnitUiKey)
            {
                DisposalUnitWindow = new DisposalUnitWindow();

                DisposalUnitWindow.OpenCenteredRight();
                DisposalUnitWindow.OnClose += Close;

                DisposalUnitWindow.Eject.OnPressed += _ => ButtonPressed(UiButton.Eject);
                DisposalUnitWindow.Engage.OnPressed += _ => ButtonPressed(UiButton.Engage);
                DisposalUnitWindow.Power.OnPressed += _ => ButtonPressed(UiButton.Power);
            }
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not MailingUnitBoundUserInterfaceState && state is not DisposalUnitBoundUserInterfaceState)
            {
                return;
            }

            switch (state)
            {
                case MailingUnitBoundUserInterfaceState mailingUnitState:
                    MailingUnitWindow?.UpdateState(mailingUnitState);
                    break;

                case DisposalUnitBoundUserInterfaceState disposalUnitState:
                    DisposalUnitWindow?.UpdateState(disposalUnitState);
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing)
                return;

            MailingUnitWindow?.Dispose();
            DisposalUnitWindow?.Dispose();
        }
    }
}