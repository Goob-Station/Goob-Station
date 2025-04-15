// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.DeviceLinking.Components;
using Content.Server.DeviceNetwork;
using SignalReceivedEvent = Content.Server.DeviceLinking.Events.SignalReceivedEvent;

namespace Content.Server.DeviceLinking.Systems;

public sealed class EdgeDetectorSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _deviceLink = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EdgeDetectorComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<EdgeDetectorComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnInit(EntityUid uid, EdgeDetectorComponent comp, ComponentInit args)
    {
        _deviceLink.EnsureSinkPorts(uid, comp.InputPort);
        _deviceLink.EnsureSourcePorts(uid, comp.OutputHighPort, comp.OutputLowPort);
    }

    private void OnSignalReceived(EntityUid uid, EdgeDetectorComponent comp, ref SignalReceivedEvent args)
    {
        // only handle signals with edges
        var state = SignalState.Momentary;
        if (args.Data == null ||
            !args.Data.TryGetValue(DeviceNetworkConstants.LogicState, out state) ||
            state == SignalState.Momentary)
            return;

        if (args.Port != comp.InputPort)
            return;

        // make sure the level changed, multiple devices sending the same level are treated as one spamming
        if (comp.State != state)
        {
            comp.State = state;

            var port = state == SignalState.High ? comp.OutputHighPort : comp.OutputLowPort;
            _deviceLink.InvokePort(uid, port);
        }
    }
}