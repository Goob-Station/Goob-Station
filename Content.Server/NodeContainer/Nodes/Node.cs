// SPDX-FileCopyrightText: 2020 ColdAutumnRain <73938872+ColdAutumnRain@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 collinlunn <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 py01 <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 py01 <pyronetics01@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.NodeGroups;
using Robust.Shared.Map.Components;

namespace Content.Server.NodeContainer.Nodes
{
    /// <summary>
    ///     Organizes themselves into distinct <see cref="INodeGroup"/>s with other <see cref="Node"/>s
    ///     that they can "reach" and have the same <see cref="Node.NodeGroupID"/>.
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public abstract partial class Node
    {
        /// <summary>
        ///     An ID used as a criteria for combining into groups. Determines which <see cref="INodeGroup"/>
        ///     implementation is used as a group, detailed in <see cref="INodeGroupFactory"/>.
        /// </summary>
        [DataField("nodeGroupID")]
        public NodeGroupID NodeGroupID { get; private set; } = NodeGroupID.Default;

        /// <summary>
        ///     The node group this node is a part of.
        /// </summary>
        [ViewVariables] public INodeGroup? NodeGroup;

        /// <summary>
        ///     The entity that owns this node via its <see cref="NodeContainerComponent"/>.
        /// </summary>
        [ViewVariables] public EntityUid Owner { get; private set; } = default!;

        /// <summary>
        ///     If this node should be considered for connection by other nodes.
        /// </summary>
        public virtual bool Connectable(IEntityManager entMan, TransformComponent? xform = null)
        {
            if (Deleting)
                return false;

            if (entMan.IsQueuedForDeletion(Owner))
                return false;

            if (!NeedAnchored)
                return true;

            xform ??= entMan.GetComponent<TransformComponent>(Owner);
            return xform.Anchored;
        }

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("needAnchored")]
        public bool NeedAnchored { get; private set; } = true;

        public virtual void OnAnchorStateChanged(IEntityManager entityManager, bool anchored) { }

        /// <summary>
        ///    Prevents a node from being used by other nodes while midway through removal.
        /// </summary>
        public bool Deleting;

        /// <summary>
        ///     All compatible nodes that are reachable by this node.
        ///     Effectively, active connections out of this node.
        /// </summary>
        public readonly HashSet<Node> ReachableNodes = new();

        internal int FloodGen;
        internal int UndirectGen;
        internal bool FlaggedForFlood;
        internal int NetId;

        /// <summary>
        ///     Name of this node on the owning <see cref="NodeContainerComponent"/>.
        /// </summary>
        public string Name = default!;

        /// <summary>
        ///     Invoked when the owning <see cref="NodeContainerComponent"/> is initialized.
        /// </summary>
        /// <param name="owner">The owning entity.</param>
        public virtual void Initialize(EntityUid owner, IEntityManager entMan)
        {
            Owner = owner;
        }

        /// <summary>
        ///     How this node will attempt to find other reachable <see cref="Node"/>s to group with.
        ///     Returns a set of <see cref="Node"/>s to consider grouping with. Should not return this current <see cref="Node"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The set of nodes returned can be asymmetrical
        /// (meaning that it can return other nodes whose <see cref="GetReachableNodes"/> does not return this node).
        /// If this is used, creation of a new node may not correctly merge networks unless both sides
        /// of this asymmetric relation are made to manually update with <see cref="NodeGroupSystem.QueueReflood"/>.
        /// </para>
        /// </remarks>
        public abstract IEnumerable<Node> GetReachableNodes(TransformComponent xform,
            EntityQuery<NodeContainerComponent> nodeQuery,
            EntityQuery<TransformComponent> xformQuery,
            MapGridComponent? grid,
            IEntityManager entMan);
    }
}