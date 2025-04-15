// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Git-Nivrak <59925169+Git-Nivrak@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 Jacob Tong <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TemporalOroboros <TemporalOroboros@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared.Maps
{
    // TODO move all these methods to LookupSystem or TurfSystem
    // That, or make the interface arguments non-optional so people stop failing to pass them in.
    public static class TurfHelpers
    {
        /// <summary>
        ///     Attempts to get the turf at a certain coordinates or null if no such turf is found.
        /// </summary>
        public static TileRef? GetTileRef(this EntityCoordinates coordinates, IEntityManager? entityManager = null, IMapManager? mapManager = null)
        {
            entityManager ??= IoCManager.Resolve<IEntityManager>();

            if (!coordinates.IsValid(entityManager))
                return null;

            mapManager ??= IoCManager.Resolve<IMapManager>();
            var pos = coordinates.ToMap(entityManager, entityManager.System<SharedTransformSystem>());
            if (!mapManager.TryFindGridAt(pos, out _, out var grid))
                return null;

            if (!grid.TryGetTileRef(coordinates, out var tile))
                return null;

            return tile;
        }

        public static bool TryGetTileRef(this EntityCoordinates coordinates, [NotNullWhen(true)] out TileRef? turf, IEntityManager? entityManager = null, IMapManager? mapManager = null)
        {
            return (turf = coordinates.GetTileRef(entityManager, mapManager)) != null;
        }

        /// <summary>
        ///     Returns the content tile definition for a tile.
        /// </summary>
        public static ContentTileDefinition GetContentTileDefinition(this Tile tile, ITileDefinitionManager? tileDefinitionManager = null)
        {
            tileDefinitionManager ??= IoCManager.Resolve<ITileDefinitionManager>();
            return (ContentTileDefinition)tileDefinitionManager[tile.TypeId];
        }

        /// <summary>
        ///     Returns whether a tile is considered space.
        /// </summary>
        public static bool IsSpace(this Tile tile, ITileDefinitionManager? tileDefinitionManager = null)
        {
            return tile.GetContentTileDefinition(tileDefinitionManager).MapAtmosphere;
        }

        /// <summary>
        ///     Returns the content tile definition for a tile ref.
        /// </summary>
        public static ContentTileDefinition GetContentTileDefinition(this TileRef tile, ITileDefinitionManager? tileDefinitionManager = null)
        {
            return tile.Tile.GetContentTileDefinition(tileDefinitionManager);
        }

        /// <summary>
        ///     Returns whether a tile ref is considered space.
        /// </summary>
        public static bool IsSpace(this TileRef tile, ITileDefinitionManager? tileDefinitionManager = null)
        {
            return tile.Tile.IsSpace(tileDefinitionManager);
        }

        /// <summary>
        ///     Helper that returns all entities in a turf.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use the lookup system")]
        public static IEnumerable<EntityUid> GetEntitiesInTile(this TileRef turf, LookupFlags flags = LookupFlags.Static, EntityLookupSystem? lookupSystem = null)
        {
            lookupSystem ??= EntitySystem.Get<EntityLookupSystem>();

            if (!GetWorldTileBox(turf, out var worldBox))
                return Enumerable.Empty<EntityUid>();

            return lookupSystem.GetEntitiesIntersecting(turf.GridUid, worldBox, flags);
        }

        /// <summary>
        ///     Helper that returns all entities in a turf.
        /// </summary>
        [Obsolete("Use the lookup system")]
        public static IEnumerable<EntityUid> GetEntitiesInTile(this EntityCoordinates coordinates, LookupFlags flags = LookupFlags.Static, EntityLookupSystem? lookupSystem = null)
        {
            var turf = coordinates.GetTileRef();

            if (turf == null)
                return Enumerable.Empty<EntityUid>();

            return GetEntitiesInTile(turf.Value, flags, lookupSystem);
        }

        /// <summary>
        /// Checks if a turf has something dense on it.
        /// </summary>
        [Obsolete("Use turf system")]
        public static bool IsBlockedTurf(this TileRef turf, bool filterMobs, EntityLookupSystem? physics = null)
        {
            CollisionGroup mask = filterMobs
                ? CollisionGroup.MobMask
                : CollisionGroup.Impassable;

            return IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<TurfSystem>().IsTileBlocked(turf, mask);
        }

        /// <summary>
        /// Creates a box the size of a tile, at the same position in the world as the tile.
        /// </summary>
        [Obsolete]
        private static bool GetWorldTileBox(TileRef turf, out Box2Rotated res)
        {
            var entManager = IoCManager.Resolve<IEntityManager>();
            var xformSystem = entManager.System<SharedTransformSystem>();

            if (entManager.TryGetComponent<MapGridComponent>(turf.GridUid, out var tileGrid))
            {
                var gridRot = xformSystem.GetWorldRotation(turf.GridUid);

                // This is scaled to 90 % so it doesn't encompass walls on other tiles.
                var tileBox = Box2.UnitCentered.Scale(0.9f);
                tileBox = tileBox.Scale(tileGrid.TileSize);
                var worldPos = tileGrid.GridTileToWorldPos(turf.GridIndices);
                tileBox = tileBox.Translated(worldPos);
                // Now tileBox needs to be rotated to match grid rotation
                res = new Box2Rotated(tileBox, gridRot, worldPos);
                return true;
            }

            // Have to "return something"
            res = Box2Rotated.UnitCentered;
            return false;
        }
    }
}