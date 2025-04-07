// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Wires;
using Content.Shared.Doors;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Wires;

namespace Content.Server.Doors;

public sealed partial class DoorSafetyWireAction : ComponentWireAction<AirlockComponent>
{
    public override Color Color { get; set; } = Color.Red;
    public override string Name { get; set; } = "wire-name-door-safety";


    [DataField("timeout")]
    private int _timeout = 30;

    public override StatusLightState? GetLightState(Wire wire, AirlockComponent comp)
        => comp.Safety ? StatusLightState.On : StatusLightState.Off;

    public override object StatusKey { get; } = AirlockWireStatus.SafetyIndicator;

    public override bool Cut(EntityUid user, Wire wire, AirlockComponent door)
    {
        WiresSystem.TryCancelWireAction(wire.Owner, PulseTimeoutKey.Key);
        EntityManager.System<SharedAirlockSystem>().SetSafety(door, false);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, AirlockComponent door)
    {
        EntityManager.System<SharedAirlockSystem>().SetSafety(door, true);
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, AirlockComponent door)
    {
        EntityManager.System<SharedAirlockSystem>().SetSafety(door, false);
        WiresSystem.StartWireAction(wire.Owner, _timeout, PulseTimeoutKey.Key, new TimedWireEvent(AwaitSafetyTimerFinish, wire));
    }

    public override void Update(Wire wire)
    {
        if (!IsPowered(wire.Owner))
        {
            WiresSystem.TryCancelWireAction(wire.Owner, PulseTimeoutKey.Key);
        }
    }

    private void AwaitSafetyTimerFinish(Wire wire)
    {
        if (!wire.IsCut)
        {
            if (EntityManager.TryGetComponent<AirlockComponent>(wire.Owner, out var door))
            {
                EntityManager.System<SharedAirlockSystem>().SetSafety(door, true);
            }
        }
    }

    private enum PulseTimeoutKey : byte
    {
        Key
    }
}