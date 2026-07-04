// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@github.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 theashtronaut <112137107+theashtronaut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Piping.Trinary.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping;
using Content.Shared.Atmos.Piping.Components;
using Content.Shared.Atmos.Piping.Trinary.Components;
using Content.Shared.Audio;
using Content.Shared.Database;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server.Atmos.Piping.Trinary.EntitySystems
{
    [UsedImplicitly]
    public sealed class GasFilterSystem : EntitySystem
    {
        [Dependency] private UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly SharedAmbientSoundSystem _ambientSoundSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GasFilterComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<GasFilterComponent, AtmosDeviceUpdateEvent>(OnFilterUpdated);
            SubscribeLocalEvent<GasFilterComponent, AtmosDeviceDisabledEvent>(OnFilterLeaveAtmosphere);
            SubscribeLocalEvent<GasFilterComponent, ActivateInWorldEvent>(OnFilterActivate);
            SubscribeLocalEvent<GasFilterComponent, GasAnalyzerScanEvent>(OnFilterAnalyzed);
            // Bound UI subscriptions
            SubscribeLocalEvent<GasFilterComponent, GasFilterChangeRateMessage>(OnTransferRateChangeMessage);
            SubscribeLocalEvent<GasFilterComponent, GasFilterSelectGasMessage>(OnSelectGasMessage);
            SubscribeLocalEvent<GasFilterComponent, GasFilterToggleStatusMessage>(OnToggleStatusMessage);

        }

        private void OnInit(EntityUid uid, GasFilterComponent filter, ComponentInit args)
        {
            UpdateAppearance(uid, filter);
        }

        private void OnFilterUpdated(EntityUid uid, GasFilterComponent filter, ref AtmosDeviceUpdateEvent args)
        {
            if (!filter.Enabled
                || !_nodeContainer.TryGetNodes(uid, filter.InletName, filter.FilterName, filter.OutletName, out PipeNode? inletNode, out PipeNode? filterNode, out PipeNode? outletNode)
                || inletNode != outletNode // Goobstation - ignore pressure if we're inline
                    && outletNode.Air.Pressure >= Atmospherics.MaxOutputPressure)
            {
                _ambientSoundSystem.SetAmbience(uid, false);
                return;
            }

            // We multiply the transfer rate in L/s by the seconds passed since the last process to get the liters.
            var transferVol = filter.TransferRate * _atmosphereSystem.PumpSpeedup() * args.dt;

            if (transferVol <= 0)
            {
                _ambientSoundSystem.SetAmbience(uid, false);
                return;
            }

            var removed = inletNode.Air.RemoveVolume(transferVol);

            if (filter.FilteredGas.HasValue)
            {
                var filteredOut = new GasMixture() { Temperature = removed.Temperature };

                filteredOut.SetMoles(filter.FilteredGas.Value, removed.GetMoles(filter.FilteredGas.Value));
                removed.SetMoles(filter.FilteredGas.Value, 0f);

                var target = filterNode.Air.Pressure < Atmospherics.MaxOutputPressure ? filterNode : inletNode;
                _atmosphereSystem.Merge(target.Air, filteredOut);
                _ambientSoundSystem.SetAmbience(uid, filteredOut.TotalMoles > 0f);
            }

            _atmosphereSystem.Merge(outletNode.Air, removed);
        }

        private void OnFilterLeaveAtmosphere(EntityUid uid, GasFilterComponent filter, ref AtmosDeviceDisabledEvent args)
        {
            filter.Enabled = false;

            UpdateAppearance(uid, filter);
            _ambientSoundSystem.SetAmbience(uid, false);

            DirtyUI(uid, filter);
            _userInterfaceSystem.CloseUi(uid, GasFilterUiKey.Key);
        }

        private void OnFilterActivate(EntityUid uid, GasFilterComponent filter, ActivateInWorldEvent args)
        {
            if (args.Handled || !args.Complex)
                return;

            if (!TryComp(args.User, out ActorComponent? actor))
                return;

            if (Comp<TransformComponent>(uid).Anchored)
            {
                _userInterfaceSystem.OpenUi(uid, GasFilterUiKey.Key, actor.PlayerSession);
                DirtyUI(uid, filter);
            }
            else
            {
                _popupSystem.PopupCursor(Loc.GetString("comp-gas-filter-ui-needs-anchor"), args.User);
            }

            args.Handled = true;
        }

        private void DirtyUI(EntityUid uid, GasFilterComponent? filter)
        {
            if (!Resolve(uid, ref filter))
                return;

            _userInterfaceSystem.SetUiState(uid, GasFilterUiKey.Key,
                new GasFilterBoundUserInterfaceState(MetaData(uid).EntityName, filter.TransferRate, filter.Enabled, filter.FilteredGas));
        }

        private void UpdateAppearance(EntityUid uid, GasFilterComponent? filter = null)
        {
            if (!Resolve(uid, ref filter, false))
                return;

            _appearanceSystem.SetData(uid, FilterVisuals.Enabled, filter.Enabled);
        }

        private void OnToggleStatusMessage(EntityUid uid, GasFilterComponent filter, GasFilterToggleStatusMessage args)
        {
            filter.Enabled = args.Enabled;
            _adminLogger.Add(LogType.AtmosPowerChanged, LogImpact.Medium,
                $"{ToPrettyString(args.Actor):player} set the power on {ToPrettyString(uid):device} to {args.Enabled}");
            DirtyUI(uid, filter);
            UpdateAppearance(uid, filter);
        }

        private void OnTransferRateChangeMessage(EntityUid uid, GasFilterComponent filter, GasFilterChangeRateMessage args)
        {
            filter.TransferRate = Math.Clamp(args.Rate, 0f, filter.MaxTransferRate);
            _adminLogger.Add(LogType.AtmosVolumeChanged, LogImpact.Medium,
                $"{ToPrettyString(args.Actor):player} set the transfer rate on {ToPrettyString(uid):device} to {args.Rate}");
            DirtyUI(uid, filter);

        }

        private void OnSelectGasMessage(EntityUid uid, GasFilterComponent filter, GasFilterSelectGasMessage args)
        {
            if (args.Gas.HasValue)
            {
                if (Enum.IsDefined(typeof(Gas), args.Gas))
                {
                    filter.FilteredGas = args.Gas;
                    _adminLogger.Add(LogType.AtmosFilterChanged, LogImpact.Medium,
                        $"{ToPrettyString(args.Actor):player} set the filter on {ToPrettyString(uid):device} to {args.Gas.ToString()}");
                    DirtyUI(uid, filter);
                }
                else
                {
                    Log.Warning($"{ToPrettyString(uid)} received GasFilterSelectGasMessage with an invalid ID: {args.Gas}");
                }
            }
            else
            {
                filter.FilteredGas = null;
                _adminLogger.Add(LogType.AtmosFilterChanged, LogImpact.Medium,
                    $"{ToPrettyString(args.Actor):player} set the filter on {ToPrettyString(uid):device} to none");
                DirtyUI(uid, filter);
            }
        }

        /// <summary>
        /// Returns the gas mixture for the gas analyzer
        /// </summary>
        private void OnFilterAnalyzed(EntityUid uid, GasFilterComponent component, GasAnalyzerScanEvent args)
        {
            args.GasMixtures ??= new List<(string, GasMixture?)>();

            // multiply by volume fraction to make sure to send only the gas inside the analyzed pipe element, not the whole pipe system
            if (_nodeContainer.TryGetNode(uid, component.InletName, out PipeNode? inlet) && inlet.Air.Volume != 0f)
            {
                var inletAirLocal = inlet.Air.Clone();
                inletAirLocal.Multiply(inlet.Volume / inlet.Air.Volume);
                inletAirLocal.Volume = inlet.Volume;
                args.GasMixtures.Add((Loc.GetString("gas-analyzer-window-text-inlet"), inletAirLocal));
            }
            if (_nodeContainer.TryGetNode(uid, component.FilterName, out PipeNode? filterNode) && filterNode.Air.Volume != 0f)
            {
                var filterNodeAirLocal = filterNode.Air.Clone();
                filterNodeAirLocal.Multiply(filterNode.Volume / filterNode.Air.Volume);
                filterNodeAirLocal.Volume = filterNode.Volume;
                args.GasMixtures.Add((Loc.GetString("gas-analyzer-window-text-filter"), filterNodeAirLocal));
            }
            if (_nodeContainer.TryGetNode(uid, component.OutletName, out PipeNode? outlet) && outlet.Air.Volume != 0f)
            {
                var outletAirLocal = outlet.Air.Clone();
                outletAirLocal.Multiply(outlet.Volume / outlet.Air.Volume);
                outletAirLocal.Volume = outlet.Volume;
                args.GasMixtures.Add((Loc.GetString("gas-analyzer-window-text-outlet"), outletAirLocal));
            }

            // Goobstation - if inlet and outlet are the same you cant get a direction from it
            if (inlet == outlet)
                return;

            args.DeviceFlipped = inlet != null && filterNode != null && inlet.CurrentPipeDirection.ToDirection() == filterNode.CurrentPipeDirection.ToDirection().GetClockwise90Degrees();
        }
    }
}
