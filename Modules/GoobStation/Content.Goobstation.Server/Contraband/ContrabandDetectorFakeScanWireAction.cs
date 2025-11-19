// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Wires;
using Content.Goobstation.Shared.Contraband;
using Content.Shared.Wires;

namespace Content.Goobstation.Server.Contraband;

[DataDefinition]
public sealed partial class ContrabandDetectorFakeScanWireAction : BaseToggleWireAction
{
    private ContrabandDetectorSystem _contrabandDetectorSystem = default!;

    public override Color Color { get; set; } = Color.CadetBlue;
    public override string Name { get; set; } = "wire-name-contraband-detector-fake-scan";
    public override object? StatusKey { get; } = ContrabandDetectorFakeScanWireKey.StatusKey;
    public override object? TimeoutKey { get; } = ContrabandDetectorFakeScanWireKey.TimeoutKey;

    public override void Initialize()
    {
        base.Initialize();

        _contrabandDetectorSystem = EntityManager.System<ContrabandDetectorSystem>();
    }

    public override StatusLightState? GetLightState(Wire wire)
    {
        if (EntityManager.TryGetComponent<ContrabandDetectorComponent>(wire.Owner, out var component))
        {
            return component.IsFalseScanning
                ? StatusLightState.Off
                : StatusLightState.On;
        }

        return StatusLightState.Off;
    }

    public override void ToggleValue(EntityUid owner, bool setting)
    {
        if (EntityManager.TryGetComponent<ContrabandDetectorComponent>(owner, out var component))
        {
            _contrabandDetectorSystem.TurnFakeScanning((owner, component));
        }
    }

    public override bool GetValue(EntityUid owner)
    {
        return EntityManager.TryGetComponent<ContrabandDetectorComponent>(owner, out var component) && !component.IsFalseScanning;
    }
}