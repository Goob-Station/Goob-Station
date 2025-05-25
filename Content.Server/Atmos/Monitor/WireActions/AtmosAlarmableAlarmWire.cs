// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 vulppine <vulppine@gmail.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.Monitor.Components;
using Content.Server.Atmos.Monitor.Systems;
using Content.Server.Wires;
using Content.Shared.Atmos.Monitor;
using Content.Shared.Wires;

namespace Content.Server.Atmos.Monitor;

public sealed partial class AtmosMonitorDeviceNetWire : ComponentWireAction<AtmosAlarmableComponent>
{
    // whether or not this wire will send out an alarm upon
    // being pulsed
    [DataField("alarmOnPulse")]
    private bool _alarmOnPulse = false;

    public override string Name { get; set; } = "wire-name-device-net";
    public override Color Color { get; set; } = Color.Orange;

    private AtmosAlarmableSystem _atmosAlarmableSystem = default!;

    public override object StatusKey { get; } = AtmosMonitorAlarmWireActionKeys.Network;

    public override StatusLightState? GetLightState(Wire wire, AtmosAlarmableComponent comp)
    {
        if (!_atmosAlarmableSystem.TryGetHighestAlert(wire.Owner, out var alarm, comp))
        {
            alarm = AtmosAlarmType.Normal;
        }

        return alarm == AtmosAlarmType.Danger
            ? StatusLightState.BlinkingFast
            : StatusLightState.On;
    }

    public override void Initialize()
    {
        base.Initialize();

        _atmosAlarmableSystem = EntityManager.System<AtmosAlarmableSystem>();
    }

    public override bool Cut(EntityUid user, Wire wire, AtmosAlarmableComponent comp)
    {
        comp.IgnoreAlarms = true;
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, AtmosAlarmableComponent comp)
    {
        comp.IgnoreAlarms = false;
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, AtmosAlarmableComponent comp)
    {
        if (_alarmOnPulse)
            _atmosAlarmableSystem.ForceAlert(wire.Owner, AtmosAlarmType.Danger, comp);
    }
}