// SPDX-FileCopyrightText: 2020 py01 <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 py01 <pyronetics01@gmail.com>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 collinlunn <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace Content.Server.NodeContainer.NodeGroups
{
    /// <summary>
    ///     Associates a <see cref="INodeGroup"/> implementation with a <see cref="NodeGroupID"/>.
    ///     This is used to gurantee all <see cref="INode"/>s of the same <see cref="NodeGroupID"/>
    ///     have the same type of <see cref="INodeGroup"/>. Used by <see cref="INodeGroupFactory"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [MeansImplicitUse]
    public sealed class NodeGroupAttribute : Attribute
    {
        public NodeGroupID[] NodeGroupIDs { get; }

        public NodeGroupAttribute(params NodeGroupID[] nodeGroupTypes)
        {
            NodeGroupIDs = nodeGroupTypes;
        }
    }
}