// SPDX-FileCopyrightText: 2024 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.Consoles;
using Robust.Shared.GameStates;

namespace Content.Client.Atmos.Consoles;

public sealed class AtmosMonitoringConsoleSystem : SharedAtmosMonitoringConsoleSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AtmosMonitoringConsoleComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, AtmosMonitoringConsoleComponent component, ref ComponentHandleState args)
    {
        Dictionary<Vector2i, Dictionary<(int, string), ulong>> modifiedChunks;
        Dictionary<NetEntity, AtmosDeviceNavMapData> atmosDevices;

        switch (args.Current)
        {
            case AtmosMonitoringConsoleDeltaState delta:
                {
                    modifiedChunks = delta.ModifiedChunks;
                    atmosDevices = delta.AtmosDevices;

                    foreach (var index in component.AtmosPipeChunks.Keys)
                    {
                        if (!delta.AllChunks!.Contains(index))
                            component.AtmosPipeChunks.Remove(index);
                    }

                    break;
                }

            case AtmosMonitoringConsoleState state:
                {
                    modifiedChunks = state.Chunks;
                    atmosDevices = state.AtmosDevices;

                    foreach (var index in component.AtmosPipeChunks.Keys)
                    {
                        if (!state.Chunks.ContainsKey(index))
                            component.AtmosPipeChunks.Remove(index);
                    }

                    break;
                }
            default:
                return;
        }

        foreach (var (origin, chunk) in modifiedChunks)
        {
            var newChunk = new AtmosPipeChunk(origin);
            newChunk.AtmosPipeData = new Dictionary<(int, string), ulong>(chunk);

            component.AtmosPipeChunks[origin] = newChunk;
        }

        component.AtmosDevices.Clear();

        foreach (var (nuid, atmosDevice) in atmosDevices)
        {
            component.AtmosDevices[nuid] = atmosDevice;
        }
    }
}