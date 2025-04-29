// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos.Components;

namespace Content.Shared.Atmos.Consoles;

public abstract partial class SharedAtmosAlertsComputerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AtmosAlertsComputerComponent, AtmosAlertsComputerDeviceSilencedMessage>(OnDeviceSilencedMessage);
    }

    private void OnDeviceSilencedMessage(EntityUid uid, AtmosAlertsComputerComponent component, AtmosAlertsComputerDeviceSilencedMessage args)
    {
        if (args.SilenceDevice)
            component.SilencedDevices.Add(args.AtmosDevice);

        else
            component.SilencedDevices.Remove(args.AtmosDevice);

        Dirty(uid, component);
    }
}