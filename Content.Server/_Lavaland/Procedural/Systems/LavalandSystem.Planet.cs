// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Atmos.Components;
using Content.Shared._Lavaland.Procedural.Prototypes;
using Content.Shared.Gravity;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Salvage;
using Content.Shared.Shuttles.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Procedural.Systems;

public sealed partial class LavalandSystem
{
    public bool SetupLavalandPlanet(
        ProtoId<LavalandMapPrototype> mapProto,
        out Entity<LavalandMapComponent>? lavaland,
        int? seed = null,
        Entity<LavalandPreloaderComponent>? preloader = null)
    {
        lavaland = null;

        if (!LavalandEnabled)
            return true;

        if (preloader == null)
        {
            preloader = GetPreloaderEntity();
            if (preloader == null)
            {
                Log.Warning("Failed to find a preloader entity when generating a new planet!");
                return false;
            }
        }

        var proto = _proto.Index(mapProto);
        var prototype = _proto.Index(proto.Planet);
        var layout = _proto.Index(proto.Layout);
        var ruinPool = _proto.Index(proto.Ruins);

        // Basic setup.
        var lavalandMap = _map.CreateMap(out var lavalandMapId, runMapInit: false);
        var mapComp = EnsureComp<LavalandMapComponent>(lavalandMap);
        lavaland = (lavalandMap, mapComp);

        // If not specified already, create new seed
        seed ??= _random.Next();

        var lavalandPrototypeId = prototype.ID;

        PlanetBasicSetup(lavalandMap, prototype, seed.Value);

        // Ensure that it's paused
        _map.SetPaused(lavalandMapId, true);

        SetupLayout(lavalandMap, lavalandMapId, layout, out mapComp.SpawnedGrids);

        var loadBox = Box2.CentredAroundZero(new Vector2(prototype.RestrictedRange, prototype.RestrictedRange));

        mapComp.Seed = seed.Value;
        mapComp.PrototypeId = lavalandPrototypeId;
        mapComp.LoadArea = loadBox;

        SetupRuins(ruinPool, lavaland.Value, preloader.Value);

        // Hide all grids from the mass scanner.
        foreach (var grid in _mapManager.GetAllGrids(lavalandMapId))
        {
            var flag = IFFFlags.HideLabel;

            /*#if DEBUG || TOOLS Uncomment me when GPS is done.
            flag = IFFFlags.HideLabel;
            #endif*/

            _shuttle.AddIFFFlag(grid, flag);
        }

        _map.InitializeMap(lavalandMapId);

        // Assign all other components to the map
        if (prototype.AddComponents != null)
            EntityManager.AddComponents(lavalandMap, prototype.AddComponents);

        // Preload here to prevent biome entities from overlaying with everything else
        _biome.Preload(lavalandMap, Comp<BiomeComponent>(lavalandMap), loadBox);

        return true;
    }

    private void PlanetBasicSetup(EntityUid lavalandMap, LavalandPlanetPrototype prototype, int seed)
    {
        // Name
        _metaData.SetEntityName(lavalandMap, Loc.GetString(prototype.Name));

        // Biomes
        _biome.EnsurePlanet(lavalandMap, _proto.Index(prototype.BiomePrototype), seed, mapLight: prototype.MapLight);

        // Marker Layers
        var biome = EnsureComp<BiomeComponent>(lavalandMap);
        foreach (var marker in prototype.OreLayers)
        {
            _biome.AddMarkerLayer(lavalandMap, biome, marker);
        }
        Dirty(lavalandMap, biome);

        // Gravity
        var gravity = EnsureComp<GravityComponent>(lavalandMap);
        gravity.Enabled = true;
        Dirty(lavalandMap, gravity);

        var atmos = EnsureComp<MapAtmosphereComponent>(lavalandMap);
        _atmos.SetMapGasMixture(lavalandMap, prototype.Atmosphere, atmos);

        // Restricted Range
        var restricted = EnsureComp<RestrictedRangeComponent>(lavalandMap);
        restricted.Range = prototype.RestrictedRange;
    }

    private void SetupLayout(EntityUid lavaland, MapId lavalandMapId, LavalandLayoutPrototype? proto, out List<EntityUid> spawned)
    {
        spawned = new();

        if (proto == null)
            return; // nothing to spawn

        foreach (var layout in proto.Layouts)
        {
            if (!_mapLoader.TryLoadGrid(lavalandMapId, layout.GridPath, out var result))
            {
                Log.Error($"Failed to load grid {layout.GridPath} on planet {ToPrettyString(lavaland)}!");
                continue;
            }

            _transform.SetCoordinates(result.Value, new EntityCoordinates(lavaland, layout.Position));
            _metaData.SetEntityName(result.Value, Loc.GetString(layout.Name));

            Log.Debug($"Spawned {ToPrettyString(result.Value)} grid on planet {ToPrettyString(lavaland)}.");
            spawned.Add(result.Value);
        }
    }
}
