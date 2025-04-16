// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.NodeContainer.NodeGroups;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Power.Components;

namespace Content.Server.Power.NodeGroups
{
    public abstract class BaseNetConnectorNodeGroup<TNetType> : BaseNodeGroup
    {
        protected IEntityManager EntMan = default!;

        public override void Initialize(Node sourceNode, IEntityManager entMan)
        {
            base.Initialize(sourceNode, entMan);
            EntMan = entMan;
        }

        public override void LoadNodes(List<Node> groupNodes)
        {
            base.LoadNodes(groupNodes);

            foreach (var node in groupNodes)
            {
                // TODO POWER PERFORMANCE
                // Replace this with TryComps or some other sane way of doing this, the current solution is awful.
                // This allocates an array, copies ALL of an entities components over, and then iterates over them to
                // yield any that implement the interface.
                foreach (var comp in EntMan.GetComponents<IBaseNetConnectorComponent<TNetType>>(node.Owner))
                {
                    if ((comp.NodeId == null ||
                         comp.NodeId == node.Name) &&
                        (NodeGroupID) comp.Voltage == node.NodeGroupID)
                    {
                        SetNetConnectorNet(comp);
                    }
                }
            }
        }

        protected abstract void SetNetConnectorNet(IBaseNetConnectorComponent<TNetType> netConnectorComponent);
    }
}