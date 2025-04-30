// SPDX-FileCopyrightText: 2021 Julian Giebel <j.giebel@netrocks.info>
// SPDX-FileCopyrightText: 2021 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.DeviceNetwork.Components;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.EntitySystems;
using JetBrains.Annotations;
using Content.Server.Power.EntitySystems;
using Content.Server.Power.Nodes;

namespace Content.Server.DeviceNetwork.Systems
{
    [UsedImplicitly]
    public sealed class ApcNetworkSystem : EntitySystem
    {
        [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ApcNetworkComponent, BeforePacketSentEvent>(OnBeforePacketSent);

            SubscribeLocalEvent<ApcNetworkComponent, ExtensionCableSystem.ProviderConnectedEvent>(OnProviderConnected);
            SubscribeLocalEvent<ApcNetworkComponent, ExtensionCableSystem.ProviderDisconnectedEvent>(OnProviderDisconnected);
        }

        /// <summary>
        /// Checks if both devices are connected to the same apc
        /// </summary>
        private void OnBeforePacketSent(EntityUid uid, ApcNetworkComponent receiver, BeforePacketSentEvent args)
        {
            if (!EntityManager.TryGetComponent(args.Sender, out ApcNetworkComponent? sender)) return;

            if (sender.ConnectedNode?.NodeGroup == null || !sender.ConnectedNode.NodeGroup.Equals(receiver.ConnectedNode?.NodeGroup))
            {
                args.Cancel();
            }
        }

        private void OnProviderConnected(EntityUid uid, ApcNetworkComponent component, ExtensionCableSystem.ProviderConnectedEvent args)
        {
            if (!EntityManager.TryGetComponent(args.Provider.Owner, out NodeContainerComponent? nodeContainer)) return;

            if (_nodeContainer.TryGetNode(nodeContainer, "power", out CableNode? node))
            {
                component.ConnectedNode = node;
            }
            else if (_nodeContainer.TryGetNode(nodeContainer, "output", out CableDeviceNode? deviceNode))
            {
                component.ConnectedNode = deviceNode;
            }

        }

        private void OnProviderDisconnected(EntityUid uid, ApcNetworkComponent component, ExtensionCableSystem.ProviderDisconnectedEvent args)
        {
            component.ConnectedNode = null;
        }
    }
}