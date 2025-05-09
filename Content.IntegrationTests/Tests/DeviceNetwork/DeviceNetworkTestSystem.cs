// SPDX-FileCopyrightText: 2021 Julian Giebel <j.giebel@netrocks.info>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.DeviceNetwork;
using Robust.Shared.GameObjects;
using Robust.Shared.Reflection;

namespace Content.IntegrationTests.Tests.DeviceNetwork
{
    [Reflect(false)]
    public sealed class DeviceNetworkTestSystem : EntitySystem
    {
        public NetworkPayload LastPayload = default;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DeviceNetworkComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
        }

        public void SendBaselineTestEvent(EntityUid uid)
        {
            RaiseLocalEvent(uid, new DeviceNetworkPacketEvent(0, "", 0, "", uid, new NetworkPayload()));
        }

        private void OnPacketReceived(EntityUid uid, DeviceNetworkComponent component, DeviceNetworkPacketEvent args)
        {
            LastPayload = args.Data;
        }
    }
}