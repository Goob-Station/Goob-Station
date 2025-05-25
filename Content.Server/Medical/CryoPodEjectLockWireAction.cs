// SPDX-FileCopyrightText: 2022 Francesco <frafonia@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Wires;
using Content.Shared.Medical.Cryogenics;
using Content.Shared.Wires;

namespace Content.Server.Medical;

/// <summary>
/// Causes a failure in the cryo pod ejection system when cut. A crowbar will be needed to pry open the pod.
/// </summary>
public sealed partial class CryoPodEjectLockWireAction: ComponentWireAction<CryoPodComponent>
{
    public override Color Color { get; set; } = Color.Red;
    public override string Name { get; set; } = "wire-name-lock";
    public override bool LightRequiresPower { get; set; } = false;

    public override object? StatusKey { get; } = CryoPodWireActionKey.Key;
    public override bool Cut(EntityUid user, Wire wire, CryoPodComponent cryoPodComponent)
    {
        if (!cryoPodComponent.PermaLocked)
            cryoPodComponent.Locked = true;

        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, CryoPodComponent cryoPodComponent)
    {
        if (!cryoPodComponent.PermaLocked)
            cryoPodComponent.Locked = false;

        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, CryoPodComponent cryoPodComponent) { }

    public override StatusLightState? GetLightState(Wire wire, CryoPodComponent comp)
        => comp.Locked ? StatusLightState.On : StatusLightState.Off;
}