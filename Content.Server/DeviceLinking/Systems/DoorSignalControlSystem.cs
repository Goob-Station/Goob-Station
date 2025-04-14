// SPDX-FileCopyrightText: 2022 Jack Fox <35575261+DubiousDoggo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2023 Sailor <109166122+malchanceux@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Kot <1192090+koteq@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.DeviceLinking.Components;
using Content.Server.DeviceNetwork;
using Content.Server.Doors.Systems;
using Content.Shared.Doors.Components;
using Content.Shared.Doors;
using JetBrains.Annotations;
using SignalReceivedEvent = Content.Server.DeviceLinking.Events.SignalReceivedEvent;

namespace Content.Server.DeviceLinking.Systems
{
    [UsedImplicitly]
    public sealed class DoorSignalControlSystem : EntitySystem
    {
        [Dependency] private readonly DoorSystem _doorSystem = default!;
        [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<DoorSignalControlComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<DoorSignalControlComponent, SignalReceivedEvent>(OnSignalReceived);
            SubscribeLocalEvent<DoorSignalControlComponent, DoorStateChangedEvent>(OnStateChanged);
        }

        private void OnInit(EntityUid uid, DoorSignalControlComponent component, ComponentInit args)
        {

            _signalSystem.EnsureSinkPorts(uid, component.OpenPort, component.ClosePort, component.TogglePort);
            _signalSystem.EnsureSourcePorts(uid, component.OutOpen);
        }

        private void OnSignalReceived(EntityUid uid, DoorSignalControlComponent component, ref SignalReceivedEvent args)
        {
            if (!TryComp(uid, out DoorComponent? door))
                return;

            var state = SignalState.Momentary;
            args.Data?.TryGetValue(DeviceNetworkConstants.LogicState, out state);


            if (args.Port == component.OpenPort)
            {
                if (state == SignalState.High || state == SignalState.Momentary)
                {
                    if (door.State == DoorState.Closed)
                        _doorSystem.TryOpen(uid, door);
                }
            }
            else if (args.Port == component.ClosePort)
            {
                if (state == SignalState.High || state == SignalState.Momentary)
                {
                    if (door.State == DoorState.Open)
                        _doorSystem.TryClose(uid, door);
                }
            }
            else if (args.Port == component.TogglePort)
            {
                if (state == SignalState.High || state == SignalState.Momentary)
                {
                    _doorSystem.TryToggleDoor(uid, door);
                }
            }
            else if (args.Port == component.InBolt)
            {
                if (!TryComp<DoorBoltComponent>(uid, out var bolts))
                    return;

                // if its a pulse toggle, otherwise set bolts to high/low
                bool bolt;
                if (state == SignalState.Momentary)
                {
                    bolt = !bolts.BoltsDown;
                }
                else
                {
                    bolt = state == SignalState.High;
                }

                _doorSystem.SetBoltsDown((uid, bolts), bolt);
            }
        }

        private void OnStateChanged(EntityUid uid, DoorSignalControlComponent door, DoorStateChangedEvent args)
        {
            if (args.State == DoorState.Closed)
            {
                // only ever say the door is closed when it is completely airtight
                _signalSystem.SendSignal(uid, door.OutOpen, false);
            }
            else if (args.State == DoorState.Open
                  || args.State == DoorState.Opening
                  || args.State == DoorState.Closing
                  || args.State == DoorState.Emagging)
            {
                // say the door is open whenever it would be letting air pass
                _signalSystem.SendSignal(uid, door.OutOpen, true);
            }
        }
    }
}