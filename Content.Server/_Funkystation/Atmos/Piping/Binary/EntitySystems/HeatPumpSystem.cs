// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Binary.Components;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Audio;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Power.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping.Binary.Components;
using Content.Shared.Atmos.Piping.Components;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server.Atmos.Piping.Binary.EntitySystems
{
    [UsedImplicitly]
    public sealed class HeatPumpSystem : EntitySystem
    {
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly AmbientSoundSystem _ambientSoundSystem = default!;
        [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
        [Dependency] private readonly AtmosphereSystem _atmos = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<HeatPumpComponent, ActivateInWorldEvent>(OnActivate);
            SubscribeLocalEvent<HeatPumpComponent, AtmosDeviceUpdateEvent>(OnAtmosUpdate);
            SubscribeLocalEvent<HeatPumpComponent, ExaminedEvent>(OnExamined);
            SubscribeLocalEvent<HeatPumpComponent, AtmosDeviceDisabledEvent>(OnHeatPumpLeaveAtmosphere);
            SubscribeLocalEvent<HeatPumpComponent, PowerChangedEvent>(OnPowerChanged);
            // Bound UI subscriptions
            SubscribeLocalEvent<HeatPumpComponent, GasHeatPumpChangeTransferRateMessage>(OnTransferRateChangeMessage);
            SubscribeLocalEvent<HeatPumpComponent, GasHeatPumpToggleStatusMessage>(OnToggleStatusMessage);
        }

        private void UpdateAppearance(Entity<HeatPumpComponent> pump)
            => _appearanceSystem.SetData(pump, HeatPumpVisuals.Enabled, pump.Comp.Active && _powerReceiverSystem.IsPowered(pump.Owner));

        private void OnExamined(Entity<HeatPumpComponent> ent, ref ExaminedEvent args)
        {
            var pump = ent.Comp;
            if (!Comp<TransformComponent>(ent).Anchored || !args.IsInDetailsRange) // Not anchored? Out of range? No status.
                return;

            if (Loc.TryGetString("heat-pump-examined", out var str,
                    ("statusColor", pump.Active ? "green" : "orange"),
                    ("on", pump.Active)))
            {
                args.PushMarkup(str);
            }
        }

        private void OnAtmosUpdate(Entity<HeatPumpComponent> pump, ref AtmosDeviceUpdateEvent args)
        {
            var pumpComponent = pump.Comp;
            if (!pumpComponent.Active ||
                !_powerReceiverSystem.IsPowered(pump) ||
                !_nodeContainer.TryGetNodes(pump.Owner, pumpComponent.InletName, pumpComponent.OutletName, out PipeNode? inlet, out PipeNode? outlet))
            {
                _ambientSoundSystem.SetAmbience(pump, false);
                return;
            }

            var airInput = inlet.Air;
            var airOutput = outlet.Air;

            if (airInput.TotalMoles <= 0 || airOutput.TotalMoles <= 0)
                return;

            var removeInput = airInput.RemoveRatio(0.9f);
            var removeOutput = airOutput.RemoveRatio(0.9f);

            _ambientSoundSystem.SetAmbience(pump, removeInput.Temperature > removeOutput.Temperature && pumpComponent.TransferRate > 0);

            var coolantTemperatureDelta = removeInput.Temperature - removeOutput.Temperature;

            if (coolantTemperatureDelta > 0)
            {
                var inputCapacity = _atmos.GetHeatCapacity(removeInput, true);
                var outputCapacity = _atmos.GetHeatCapacity(removeOutput, true);

                var coolingHeatAmount = (pumpComponent.TransferRate * pumpComponent.TransferCoefficient) * CalculateConductionEnergy(coolantTemperatureDelta, outputCapacity, inputCapacity);
                removeOutput.Temperature = MathF.Max(removeOutput.Temperature + (coolingHeatAmount / outputCapacity), Atmospherics.TCMB);
                removeInput.Temperature = MathF.Max(removeInput.Temperature - (coolingHeatAmount / inputCapacity), Atmospherics.TCMB);
            }

            _atmos.Merge(airInput, removeInput);
            _atmos.Merge(airOutput, removeOutput);
        }

        private void OnPowerChanged(Entity<HeatPumpComponent> pump, ref PowerChangedEvent args)
        {
            if (!args.Powered)
                _ambientSoundSystem.SetAmbience(pump.Owner, false);
        }

        private void OnActivate(Entity<HeatPumpComponent> pump, ref ActivateInWorldEvent args)
        {
            if (args.Handled || !args.Complex)
                return;

            if (!TryComp(args.User, out ActorComponent? actor))
                return;

            if (Transform(pump).Anchored)
            {
                _userInterfaceSystem.OpenUi(pump.Owner, GasHeatPumpUiKey.Key, actor.PlayerSession);
                DirtyUI(pump);
            }
            else
                _popup.PopupCursor(Loc.GetString("comp-gas-pump-ui-needs-anchor"), args.User);

            args.Handled = true;
        }

        private void OnHeatPumpLeaveAtmosphere(Entity<HeatPumpComponent> pump, ref AtmosDeviceDisabledEvent args)
        {
            pump.Comp.Active = false;
            UpdateAppearance(pump);

            DirtyUI(pump);
            _userInterfaceSystem.CloseUi(pump.Owner, GasHeatPumpUiKey.Key);
        }

        private void OnToggleStatusMessage(Entity<HeatPumpComponent> pump, ref GasHeatPumpToggleStatusMessage args)
        {
            pump.Comp.Active = args.Enabled;
            _adminLogger.Add(LogType.AtmosPowerChanged, LogImpact.Medium,
                $"{ToPrettyString(args.Actor):player} set the power on {ToPrettyString(pump.Owner):device} to {args.Enabled}");

            UpdateAppearance(pump);
            DirtyUI(pump);
        }

        private void OnTransferRateChangeMessage(Entity<HeatPumpComponent> pump, ref GasHeatPumpChangeTransferRateMessage args)
        {
            var pumpComponent = pump.Comp;

            pumpComponent.TransferRate = Math.Clamp(args.TransferRate, 0f, pumpComponent.MaxTransferRate);
            _adminLogger.Add(LogType.AtmosVolumeChanged, LogImpact.Medium,
                $"{ToPrettyString(args.Actor):player} set the transfer rate on {ToPrettyString(pump.Owner):device} to {args.TransferRate}");

            UpdateAppearance(pump);
            DirtyUI(pump);
        }

        private void DirtyUI(Entity<HeatPumpComponent> pump)
        {
            var pumpComponent = pump.Comp;
            _userInterfaceSystem.SetUiState(pump.Owner, GasHeatPumpUiKey.Key,
                new GasHeatPumpBoundUserInterfaceState(Name(pump.Owner), pumpComponent.TransferRate, pumpComponent.Active));
        }

        private float CalculateConductionEnergy(float temperatureDelta, float heatCapacityOne, float heatCapacityTwo)
            => temperatureDelta * (heatCapacityOne * (heatCapacityTwo / (heatCapacityOne + heatCapacityTwo)));
    }
}
