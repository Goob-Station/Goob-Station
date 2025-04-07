// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TGRCDev <tgrc@tgrc.dev>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Diagnostics.CodeAnalysis;
using Content.Server.Atmos.Piping.Binary.Components;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Shared.Construction.Components;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.Atmos.Piping.Unary.EntitySystems
{
    [UsedImplicitly]
    public sealed class GasPortableSystem : EntitySystem
    {
        [Dependency] private readonly SharedMapSystem _mapSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GasPortableComponent, AnchorAttemptEvent>(OnPortableAnchorAttempt);
            // Shouldn't need re-anchored event.
            SubscribeLocalEvent<GasPortableComponent, AnchorStateChangedEvent>(OnAnchorChanged);
        }

        private void OnPortableAnchorAttempt(EntityUid uid, GasPortableComponent component, AnchorAttemptEvent args)
        {
            if (!EntityManager.TryGetComponent(uid, out TransformComponent? transform))
                return;

            // If we can't find any ports, cancel the anchoring.
            if (!FindGasPortIn(transform.GridUid, transform.Coordinates, out _))
                args.Cancel();
        }

        private void OnAnchorChanged(EntityUid uid, GasPortableComponent portable, ref AnchorStateChangedEvent args)
        {
            if (!_nodeContainer.TryGetNode(uid, portable.PortName, out PipeNode? portableNode))
                return;

            portableNode.ConnectionsEnabled = args.Anchored;
        }

        public bool FindGasPortIn(EntityUid? gridId, EntityCoordinates coordinates, [NotNullWhen(true)] out GasPortComponent? port)
        {
            port = null;

            if (!TryComp<MapGridComponent>(gridId, out var grid))
                return false;

            foreach (var entityUid in _mapSystem.GetLocal(gridId.Value, grid, coordinates))
            {
                if (EntityManager.TryGetComponent(entityUid, out port))
                {
                    return true;
                }
            }

            return false;
        }
    }
}