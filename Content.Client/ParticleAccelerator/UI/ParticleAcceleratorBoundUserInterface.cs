// SPDX-FileCopyrightText: 2020 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Singularity.Components;
using Robust.Client.UserInterface;

namespace Content.Client.ParticleAccelerator.UI
{
    public sealed class ParticleAcceleratorBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private ParticleAcceleratorControlMenu? _menu;

        public ParticleAcceleratorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _menu = this.CreateWindow<ParticleAcceleratorControlMenu>();
            _menu.SetEntity(Owner);

            _menu.OnOverallState += SendEnableMessage;
            _menu.OnPowerState += SendPowerStateMessage;
            _menu.OnScan += SendScanPartsMessage;
        }

        public void SendEnableMessage(bool enable)
        {
            SendMessage(new ParticleAcceleratorSetEnableMessage(enable));
        }

        public void SendPowerStateMessage(ParticleAcceleratorPowerState state)
        {
            SendMessage(new ParticleAcceleratorSetPowerStateMessage(state));
        }

        public void SendScanPartsMessage()
        {
            SendMessage(new ParticleAcceleratorRescanPartsMessage());
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            _menu?.DataUpdate((ParticleAcceleratorUIState) state);
        }
    }
}