// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.NodeContainer.EntitySystems;
using Content.Server.Power.Components;
using Content.Server.Power.Nodes;
using Content.Shared.Wires;
using JetBrains.Annotations;
using Robust.Shared.Map.Components;

namespace Content.Server.Power.EntitySystems
{
    [UsedImplicitly]
    public sealed class CableVisSystem : EntitySystem
    {
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CableVisComponent, NodeGroupsRebuilt>(UpdateAppearance);
        }

        private void UpdateAppearance(EntityUid uid, CableVisComponent cableVis, ref NodeGroupsRebuilt args)
        {
            if (!_nodeContainer.TryGetNode(uid, cableVis.Node, out CableNode? node))
                return;

            var transform = Transform(uid);
            if (!TryComp<MapGridComponent>(transform.GridUid, out var grid))
                return;

            var mask = WireVisDirFlags.None;
            var tile = grid.TileIndicesFor(transform.Coordinates);

            foreach (var reachable in node.ReachableNodes)
            {
                if (reachable is not CableNode)
                    continue;

                var otherTransform = Transform(reachable.Owner);
                var otherTile = grid.TileIndicesFor(otherTransform.Coordinates);
                var diff = otherTile - tile;

                mask |= diff switch
                {
                    (0, 1) => WireVisDirFlags.North,
                    (0, -1) => WireVisDirFlags.South,
                    (1, 0) => WireVisDirFlags.East,
                    (-1, 0) => WireVisDirFlags.West,
                    _ => WireVisDirFlags.None
                };
            }

            _appearance.SetData(uid, WireVisVisuals.ConnectedMask, mask);
        }
    }
}