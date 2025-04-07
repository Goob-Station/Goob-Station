// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@github.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 daerSeebaer <61566539+daerSeebaer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jake Huxell <JakeHuxell@pm.me>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Partmedia <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Monitor.Systems;
using Content.Server.Atmos.Piping.Binary.Components;
using Content.Server.Atmos.Piping.Components;
using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Power.Components;
using Content.Shared.Atmos.Piping.Binary.Components;
using Content.Shared.Atmos.Piping.Components;
using Content.Shared.Atmos.Visuals;
using Content.Shared.Audio;
using Content.Shared.Database;
using Content.Shared.DeviceNetwork;
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
    public sealed class GasVolumePumpSystem : EntitySystem
    {
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly SharedAmbientSoundSystem _ambientSoundSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
        [Dependency] private readonly DeviceNetworkSystem _deviceNetwork = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;


        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GasVolumePumpComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<GasVolumePumpComponent, AtmosDeviceUpdateEvent>(OnVolumePumpUpdated);
            SubscribeLocalEvent<GasVolumePumpComponent, AtmosDeviceDisabledEvent>(OnVolumePumpLeaveAtmosphere);
            SubscribeLocalEvent<GasVolumePumpComponent, ExaminedEvent>(OnExamined);
            SubscribeLocalEvent<GasVolumePumpComponent, ActivateInWorldEvent>(OnPumpActivate);
            SubscribeLocalEvent<GasVolumePumpComponent, PowerChangedEvent>(OnPowerChanged);
            // Bound UI subscriptions
            SubscribeLocalEvent<GasVolumePumpComponent, GasVolumePumpChangeTransferRateMessage>(OnTransferRateChangeMessage);
            SubscribeLocalEvent<GasVolumePumpComponent, GasVolumePumpToggleStatusMessage>(OnToggleStatusMessage);

            SubscribeLocalEvent<GasVolumePumpComponent, DeviceNetworkPacketEvent>(OnPacketRecv);
        }

        private void OnInit(EntityUid uid, GasVolumePumpComponent pump, ComponentInit args)
        {
            UpdateAppearance(uid, pump);
        }

        private void OnExamined(EntityUid uid, GasVolumePumpComponent pump, ExaminedEvent args)
        {
            if (!EntityManager.GetComponent<TransformComponent>(uid).Anchored || !args.IsInDetailsRange) // Not anchored? Out of range? No status.
                return;

            if (Loc.TryGetString("gas-volume-pump-system-examined", out var str,
                        ("statusColor", "lightblue"), // TODO: change with volume?
                        ("rate", pump.TransferRate)
            ))
                args.PushMarkup(str);
        }

        private void OnPowerChanged(EntityUid uid, GasVolumePumpComponent component, ref PowerChangedEvent args)
        {
            UpdateAppearance(uid, component);
        }

        private void OnVolumePumpUpdated(EntityUid uid, GasVolumePumpComponent pump, ref AtmosDeviceUpdateEvent args)
        {
            if (!pump.Enabled ||
                (TryComp<ApcPowerReceiverComponent>(uid, out var power) && !power.Powered) ||
                !_nodeContainer.TryGetNodes(uid, pump.InletName, pump.OutletName, out PipeNode? inlet, out PipeNode? outlet))
            {
                _ambientSoundSystem.SetAmbience(uid, false);
                return;
            }

            var inputStartingPressure = inlet.Air.Pressure;
            var outputStartingPressure = outlet.Air.Pressure;

            var previouslyBlocked = pump.Blocked;
            pump.Blocked = false;

            // Pump mechanism won't do anything if the pressure is too high/too low unless you overclock it.
            if ((inputStartingPressure < pump.LowerThreshold) || (outputStartingPressure > pump.HigherThreshold) && !pump.Overclocked)
            {
                pump.Blocked = true;
            }

            // Overclocked pumps can only force gas a certain amount.
            if ((outputStartingPressure - inputStartingPressure > pump.OverclockThreshold) && pump.Overclocked)
            {
                pump.Blocked = true;
            }

            if (previouslyBlocked != pump.Blocked)
                UpdateAppearance(uid, pump);
            if (pump.Blocked)
                return;

            // We multiply the transfer rate in L/s by the seconds passed since the last process to get the liters.
            var removed = inlet.Air.RemoveVolume(pump.TransferRate * _atmosphereSystem.PumpSpeedup() * args.dt);

            // Some of the gas from the mixture leaks when overclocked.
            if (pump.Overclocked)
            {
                var tile = _atmosphereSystem.GetTileMixture(uid, excite: true);

                if (tile != null)
                {
                    var leaked = removed.RemoveRatio(pump.LeakRatio);
                    _atmosphereSystem.Merge(tile, leaked);
                }
            }

            pump.LastMolesTransferred = removed.TotalMoles;

            _atmosphereSystem.Merge(outlet.Air, removed);
            _ambientSoundSystem.SetAmbience(uid, removed.TotalMoles > 0f);
        }

        private void OnVolumePumpLeaveAtmosphere(EntityUid uid, GasVolumePumpComponent pump, ref AtmosDeviceDisabledEvent args)
        {
            pump.Enabled = false;
            UpdateAppearance(uid, pump);

            DirtyUI(uid, pump);
            _userInterfaceSystem.CloseUi(uid, GasVolumePumpUiKey.Key);
        }

        private void OnPumpActivate(EntityUid uid, GasVolumePumpComponent pump, ActivateInWorldEvent args)
        {
            if (args.Handled || !args.Complex)
                return;

            if (!EntityManager.TryGetComponent(args.User, out ActorComponent? actor))
                return;

            if (Transform(uid).Anchored)
            {
                _userInterfaceSystem.OpenUi(uid, GasVolumePumpUiKey.Key, actor.PlayerSession);
                DirtyUI(uid, pump);
            }
            else
            {
                _popup.PopupCursor(Loc.GetString("comp-gas-pump-ui-needs-anchor"), args.User);
            }

            args.Handled = true;
        }

        private void OnToggleStatusMessage(EntityUid uid, GasVolumePumpComponent pump, GasVolumePumpToggleStatusMessage args)
        {
            pump.Enabled = args.Enabled;
            _adminLogger.Add(LogType.AtmosPowerChanged, LogImpact.Medium,
                $"{ToPrettyString(args.Actor):player} set the power on {ToPrettyString(uid):device} to {args.Enabled}");
            DirtyUI(uid, pump);
            UpdateAppearance(uid, pump);
        }

        private void OnTransferRateChangeMessage(EntityUid uid, GasVolumePumpComponent pump, GasVolumePumpChangeTransferRateMessage args)
        {
            pump.TransferRate = Math.Clamp(args.TransferRate, 0f, pump.MaxTransferRate);
            _adminLogger.Add(LogType.AtmosVolumeChanged, LogImpact.Medium,
                $"{ToPrettyString(args.Actor):player} set the transfer rate on {ToPrettyString(uid):device} to {args.TransferRate}");
            DirtyUI(uid, pump);
        }

        private void DirtyUI(EntityUid uid, GasVolumePumpComponent? pump)
        {
            if (!Resolve(uid, ref pump))
                return;

            _userInterfaceSystem.SetUiState(uid, GasVolumePumpUiKey.Key,
                new GasVolumePumpBoundUserInterfaceState(Name(uid), pump.TransferRate, pump.Enabled));
        }

        private void UpdateAppearance(EntityUid uid, GasVolumePumpComponent? pump = null, AppearanceComponent? appearance = null)
        {
            if (!Resolve(uid, ref pump, ref appearance, false))
                return;

            bool pumpOn = pump.Enabled && (TryComp<ApcPowerReceiverComponent>(uid, out var power) && power.Powered);
            if (!pumpOn)
                _appearance.SetData(uid, GasVolumePumpVisuals.State, GasVolumePumpState.Off, appearance);
            else if (pump.Blocked)
                _appearance.SetData(uid, GasVolumePumpVisuals.State, GasVolumePumpState.Blocked, appearance);
            else
                _appearance.SetData(uid, GasVolumePumpVisuals.State, GasVolumePumpState.On, appearance);
        }

        private void OnPacketRecv(EntityUid uid, GasVolumePumpComponent component, DeviceNetworkPacketEvent args)
        {
            if (!TryComp(uid, out DeviceNetworkComponent? netConn)
                || !args.Data.TryGetValue(DeviceNetworkConstants.Command, out var cmd))
                return;

            var payload = new NetworkPayload();

            switch (cmd)
            {
                case AtmosDeviceNetworkSystem.SyncData:
                    payload.Add(DeviceNetworkConstants.Command, AtmosDeviceNetworkSystem.SyncData);
                    payload.Add(AtmosDeviceNetworkSystem.SyncData, new GasVolumePumpData(component.LastMolesTransferred));

                    _deviceNetwork.QueuePacket(uid, args.SenderAddress, payload, device: netConn);
                    return;
            }
        }
    }
}