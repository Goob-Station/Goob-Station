// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Binary.Components;
using Content.Server.Atmos.Piping.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Power.Components;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping.Binary.Components;
using Content.Shared.Atmos.Piping.Components;
using Content.Shared.Audio;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server.Atmos.Piping.Binary.EntitySystems
{
    [UsedImplicitly]
    public sealed class HeatPumpSystem : EntitySystem
    {
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly SharedAmbientSoundSystem _ambientSoundSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
        [Dependency] private readonly AtmosphereSystem _atmos = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        private const float MaxHeatTransferRate = 100f;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<HeatPumpComponent, ActivateInWorldEvent>(OnActivate);
            SubscribeLocalEvent<HeatPumpComponent, AtmosDeviceUpdateEvent>(OnAtmosUpdate);
            SubscribeLocalEvent<HeatPumpComponent, ExaminedEvent>(OnExamined);
            SubscribeLocalEvent<HeatPumpComponent, AtmosDeviceDisabledEvent>(OnHeatPumpLeaveAtmosphere);
            // Bound UI subscriptions
            SubscribeLocalEvent<HeatPumpComponent, GasHeatPumpChangeTransferRateMessage>(OnTransferRateChangeMessage);
            SubscribeLocalEvent<HeatPumpComponent, GasHeatPumpToggleStatusMessage>(OnToggleStatusMessage);
        }

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

        private void OnAtmosUpdate(EntityUid uid, HeatPumpComponent pump, AtmosDeviceUpdateEvent args)
        {
            if (!pump.Active ||
                (TryComp<ApcPowerReceiverComponent>(uid, out var power) && !power.Powered) ||
                !_nodeContainer.TryGetNodes(uid, pump.InletName, pump.OutletName, out PipeNode? inlet, out PipeNode? outlet))
            {
                _ambientSoundSystem.SetAmbience(uid, false);
                return;
            }

            var airInput = inlet.Air;
            var airOutput = outlet.Air;

            if (airInput.TotalMoles <= 0 || airOutput.TotalMoles <= 0)
                return;

            var removeInput = airInput.RemoveRatio(0.9f);
            var removeOutput = airOutput.RemoveRatio(0.9f);

            _ambientSoundSystem.SetAmbience(uid, removeInput.Temperature > removeOutput.Temperature && pump.TransferRate > 0);

            var coolantTemperatureDelta = removeInput.Temperature - removeOutput.Temperature;

            if (coolantTemperatureDelta > 0)
            {
                var inputCapacity = _atmos.GetHeatCapacity(removeInput, true);
                var outputCapacity = _atmos.GetHeatCapacity(removeOutput, true);

                var coolingHeatAmount = (pump.TransferRate * 0.01f) * CalculateConductionEnergy(coolantTemperatureDelta, outputCapacity, inputCapacity);
                removeOutput.Temperature = MathF.Max(removeOutput.Temperature + (coolingHeatAmount / outputCapacity), Atmospherics.TCMB);
                removeInput.Temperature = MathF.Max(removeInput.Temperature - (coolingHeatAmount / inputCapacity), Atmospherics.TCMB);
            }

            _atmos.Merge(airInput, removeInput);
            _atmos.Merge(airOutput, removeOutput);
        }

        private void OnActivate(EntityUid uid, HeatPumpComponent pump, ActivateInWorldEvent args)
        {
            if (args.Handled || !args.Complex)
                return;

            if (!EntityManager.TryGetComponent(args.User, out ActorComponent? actor))
                return;

            if (Transform(uid).Anchored)
            {
                _userInterfaceSystem.OpenUi(uid, GasHeatPumpUiKey.Key, actor.PlayerSession);
                DirtyUI(uid, pump);
            }
            else
                _popup.PopupCursor(Loc.GetString("comp-gas-pump-ui-needs-anchor"), args.User);

            args.Handled = true;
        }

        private void OnHeatPumpLeaveAtmosphere(EntityUid uid, HeatPumpComponent pump, ref AtmosDeviceDisabledEvent args)
        {
            pump.Active = false;

            DirtyUI(uid, pump);
            _userInterfaceSystem.CloseUi(uid, GasHeatPumpUiKey.Key);
        }

        private void OnToggleStatusMessage(EntityUid uid, HeatPumpComponent pump, GasHeatPumpToggleStatusMessage args)
        {
            pump.Active = args.Enabled;
            _adminLogger.Add(LogType.AtmosPowerChanged, LogImpact.Medium,
                $"{ToPrettyString(args.Actor):player} set the power on {ToPrettyString(uid):device} to {args.Enabled}");
            DirtyUI(uid, pump);
        }

        private void OnTransferRateChangeMessage(EntityUid uid, HeatPumpComponent pump, GasHeatPumpChangeTransferRateMessage args)
        {
            pump.TransferRate = Math.Clamp(args.TransferRate, 0f, pump.MaxTransferRate);
            _adminLogger.Add(LogType.AtmosVolumeChanged, LogImpact.Medium,
                $"{ToPrettyString(args.Actor):player} set the transfer rate on {ToPrettyString(uid):device} to {args.TransferRate}");
            DirtyUI(uid, pump);
        }

        private void DirtyUI(EntityUid uid, HeatPumpComponent? pump)
        {
            if (!Resolve(uid, ref pump))
                return;

            _userInterfaceSystem.SetUiState(uid, GasHeatPumpUiKey.Key,
                new GasHeatPumpBoundUserInterfaceState(Name(uid), pump.TransferRate, pump.Active));
        }

        private float CalculateConductionEnergy(float temperatureDelta, float heatCapacityOne, float heatCapacityTwo)
        {
            return temperatureDelta * (heatCapacityOne * (heatCapacityTwo / (heatCapacityOne + heatCapacityTwo)));
        }
    }
}
