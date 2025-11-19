// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 PJB3005 <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Server.DeviceNetwork.Components;
using Content.Shared.Administration;
using Content.Shared.DeviceNetwork.Components;
using Robust.Shared.Console;

namespace Content.Goobstation.Server.DeviceNetwork;

/// <summary>
///     Command for synchronizing DeviceList and DeviceNetwork
/// </summary>

[AdminCommand(AdminFlags.Mapping)]
public sealed class DeviceListSyncCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    public override string Command => "synchronizedevicelists";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length > 0)
        {
            shell.WriteError("This command takes no arguments!");
            return;
       }

        var deviceListQuery = _entityManager.AllEntityQueryEnumerator<DeviceListComponent>();
        var updatedDevices = 0;

        while (deviceListQuery.MoveNext(out var listEnt, out var listComp))
        {
            foreach (var device in listComp.Devices)
            {
                if (!_entityManager.TryGetComponent(device, out DeviceNetworkComponent? networkComp) || networkComp.DeviceLists.Contains(listEnt))
                    continue;

                networkComp.DeviceLists.Add(listEnt);
                updatedDevices++;
            }
        }

        shell.WriteLine($"Successfully synchronized {updatedDevices} devices.");
    }
}