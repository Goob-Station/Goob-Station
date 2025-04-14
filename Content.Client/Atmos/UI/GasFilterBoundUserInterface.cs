// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@github.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Kot <1192090+koteq@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping.Trinary.Components;
using Content.Shared.Localizations;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Atmos.UI
{
    /// <summary>
    /// Initializes a <see cref="GasFilterWindow"/> and updates it when new server messages are received.
    /// </summary>
    [UsedImplicitly]
    public sealed class GasFilterBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private const float MaxTransferRate = Atmospherics.MaxTransferRate;

        [ViewVariables]
        private GasFilterWindow? _window;

        public GasFilterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            var atmosSystem = EntMan.System<AtmosphereSystem>();

            _window = this.CreateWindow<GasFilterWindow>();
            _window.PopulateGasList(atmosSystem.Gases);

            _window.ToggleStatusButtonPressed += OnToggleStatusButtonPressed;
            _window.FilterTransferRateChanged += OnFilterTransferRatePressed;
            _window.SelectGasPressed += OnSelectGasPressed;
        }

        private void OnToggleStatusButtonPressed()
        {
            if (_window is null) return;
            SendMessage(new GasFilterToggleStatusMessage(_window.FilterStatus));
        }

        private void OnFilterTransferRatePressed(string value)
        {
            var rate = UserInputParser.TryFloat(value, out var parsed) ? parsed : 0f;

            SendMessage(new GasFilterChangeRateMessage(rate));
        }

        private void OnSelectGasPressed()
        {
            if (_window is null) return;
            if (_window.SelectedGas is null)
            {
                SendMessage(new GasFilterSelectGasMessage(null));
            }
            else
            {
                if (!int.TryParse(_window.SelectedGas, out var gas)) return;
                SendMessage(new GasFilterSelectGasMessage(gas));
            }
        }

        /// <summary>
        /// Update the UI state based on server-sent info
        /// </summary>
        /// <param name="state"></param>
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            if (_window == null || state is not GasFilterBoundUserInterfaceState cast)
                return;

            _window.Title = (cast.FilterLabel);
            _window.SetFilterStatus(cast.Enabled);
            _window.SetTransferRate(cast.TransferRate);
            if (cast.FilteredGas is not null)
            {
                var atmos = EntMan.System<AtmosphereSystem>();
                var gas = atmos.GetGas((Gas) cast.FilteredGas);
                var gasName = Loc.GetString(gas.Name);
                _window.SetGasFiltered(gas.ID, gasName);
            }
            else
            {
                _window.SetGasFiltered(null, Loc.GetString("comp-gas-filter-ui-filter-gas-none"));
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            _window?.Dispose();
        }
    }
}