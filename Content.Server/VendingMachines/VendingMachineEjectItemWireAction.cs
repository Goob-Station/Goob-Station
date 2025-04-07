// SPDX-FileCopyrightText: 2022 Andreas Kämper <andreas@kaemper.tech>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Wires;
using Content.Shared.VendingMachines;
using Content.Shared.Wires;

namespace Content.Server.VendingMachines;

public sealed partial class VendingMachineEjectItemWireAction : ComponentWireAction<VendingMachineComponent>
{
    private VendingMachineSystem _vendingMachineSystem = default!;

    public override Color Color { get; set; } = Color.Red;
    public override string Name { get; set; } = "wire-name-vending-eject";

    public override object? StatusKey { get; } = EjectWireKey.StatusKey;

    public override StatusLightState? GetLightState(Wire wire, VendingMachineComponent comp)
        => comp.CanShoot ? StatusLightState.BlinkingFast : StatusLightState.On;

    public override void Initialize()
    {
        base.Initialize();

        _vendingMachineSystem = EntityManager.System<VendingMachineSystem>();
    }

    public override bool Cut(EntityUid user, Wire wire, VendingMachineComponent vending)
    {
        _vendingMachineSystem.SetShooting(wire.Owner, true, vending);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, VendingMachineComponent vending)
    {
        _vendingMachineSystem.SetShooting(wire.Owner, false, vending);
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, VendingMachineComponent vending)
    {
        _vendingMachineSystem.EjectRandom(wire.Owner, true, vendComponent: vending);
    }
}