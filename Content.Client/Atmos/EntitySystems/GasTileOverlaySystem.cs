// SPDX-FileCopyrightText: 2020 Campbell Suter <znix@znix.xyz>
// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2020 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2020 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 silicons <2003111+silicons@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 GraniteSidewalk <32942106+GraniteSidewalk@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 ScalyChimp <72841710+scaly-chimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 hubismal <47284081+hubismal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TemporalOroboros <TemporalOroboros@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Atmos.Overlays;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.EntitySystems;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.GameStates;

namespace Content.Client.Atmos.EntitySystems
{
    [UsedImplicitly]
    public sealed class GasTileOverlaySystem : SharedGasTileOverlaySystem
    {
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IOverlayManager _overlayMan = default!;
        [Dependency] private readonly SpriteSystem _spriteSys = default!;
        [Dependency] private readonly SharedTransformSystem _xformSys = default!;

        private GasTileOverlay _overlay = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeNetworkEvent<GasOverlayUpdateEvent>(HandleGasOverlayUpdate);
            SubscribeLocalEvent<GasTileOverlayComponent, ComponentHandleState>(OnHandleState);

            _overlay = new GasTileOverlay(this, EntityManager, _resourceCache, ProtoMan, _spriteSys, _xformSys);
            _overlayMan.AddOverlay(_overlay);
        }

        public override void Shutdown()
        {
            base.Shutdown();
            _overlayMan.RemoveOverlay<GasTileOverlay>();
        }

        private void OnHandleState(EntityUid gridUid, GasTileOverlayComponent comp, ref ComponentHandleState args)
        {
            Dictionary<Vector2i, GasOverlayChunk> modifiedChunks;

            switch (args.Current)
            {
                // is this a delta or full state?
                case GasTileOverlayDeltaState delta:
                {
                    modifiedChunks = delta.ModifiedChunks;
                    foreach (var index in comp.Chunks.Keys)
                    {
                        if (!delta.AllChunks.Contains(index))
                            comp.Chunks.Remove(index);
                    }

                    break;
                }
                case GasTileOverlayState state:
                {
                    modifiedChunks = state.Chunks;
                    foreach (var index in comp.Chunks.Keys)
                    {
                        if (!state.Chunks.ContainsKey(index))
                            comp.Chunks.Remove(index);
                    }

                    break;
                }
                default:
                    return;
            }

            foreach (var (index, data) in modifiedChunks)
            {
                comp.Chunks[index] = data;
            }
        }

        private void HandleGasOverlayUpdate(GasOverlayUpdateEvent ev)
        {
            foreach (var (nent, removedIndicies) in ev.RemovedChunks)
            {
                var grid = GetEntity(nent);

                if (!TryComp(grid, out GasTileOverlayComponent? comp))
                    continue;

                foreach (var index in removedIndicies)
                {
                    comp.Chunks.Remove(index);
                }
            }

            foreach (var (nent, gridData) in ev.UpdatedChunks)
            {
                var grid = GetEntity(nent);

                if (!TryComp(grid, out GasTileOverlayComponent? comp))
                    continue;

                foreach (var chunkData in gridData)
                {
                    comp.Chunks[chunkData.Index] = chunkData;
                }
            }
        }
    }
}