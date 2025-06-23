// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.Piping.Binary.Components;
using Content.Shared.Atmos.Piping.Unary.Components;
using Content.Shared.IdentityManagement;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Atmos.UI
{
    /// <summary>
    /// Initializes a <see cref="GasCanisterWindow"/> and updates it when new server messages are received.
    /// </summary>
    [UsedImplicitly]
    public sealed class GasCanisterBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private GasCanisterWindow? _window;

        public GasCanisterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<GasCanisterWindow>();

            _window.ReleaseValveCloseButtonPressed += OnReleaseValveClosePressed;
            _window.ReleaseValveOpenButtonPressed += OnReleaseValveOpenPressed;
            _window.ReleasePressureSet += OnReleasePressureSet;
            _window.TankEjectButtonPressed += OnTankEjectPressed;
        }

        private void OnTankEjectPressed()
        {
            SendPredictedMessage(new GasCanisterHoldingTankEjectMessage());
        }

        private void OnReleasePressureSet(float value)
        {
            SendPredictedMessage(new GasCanisterChangeReleasePressureMessage(value));
        }

        private void OnReleaseValveOpenPressed()
        {
            SendPredictedMessage(new GasCanisterChangeReleaseValveMessage(true));
        }

        private void OnReleaseValveClosePressed()
        {
            SendPredictedMessage(new GasCanisterChangeReleaseValveMessage(false));
        }

        /// <summary>
        /// Update the UI state based on server-sent info
        /// </summary>
        /// <param name="state"></param>
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            if (_window == null || state is not GasCanisterBoundUserInterfaceState cast || !EntMan.TryGetComponent(Owner, out GasCanisterComponent? component))
                return;

            var canisterLabel = Identity.Name(Owner, EntMan);
            var tankLabel = component.GasTankSlot.Item != null ? Identity.Name(component.GasTankSlot.Item.Value, EntMan) : null;

            _window.SetCanisterLabel(canisterLabel);
            _window.SetCanisterPressure(cast.CanisterPressure);
            _window.SetPortStatus(cast.PortStatus);

            _window.SetTankLabel(tankLabel);
            _window.SetTankPressure(cast.TankPressure);
            _window.SetReleasePressureRange(component.MinReleasePressure, component.MaxReleasePressure);
            _window.SetReleasePressure(component.ReleasePressure);
            _window.SetReleaseValve(component.ReleaseValve);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            _window?.Dispose();
        }
    }
}