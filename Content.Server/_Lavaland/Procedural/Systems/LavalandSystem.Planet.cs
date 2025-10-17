// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Atmos.Components;
using Content.Shared._Lavaland.Procedural.Prototypes;
using Content.Shared.Atmos;
using Content.Shared.Gravity;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Salvage;
using Content.Shared.Shuttles.Components;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server._Lavaland.Procedural.Systems;

public sealed partial class LavalandSystem
{
    public bool SetupLavalandPlanet(
        out Entity<LavalandMapComponent>? lavaland,
        LavalandMapPrototype prototype,
        int? seed = null,
        Entity<LavalandPreloaderComponent>? preloader = null)
    {
        lavaland = null;

        if (preloader == null)
        {
            preloader = GetPreloaderEntity();
            if (preloader == null)
                return false;
        }

        // Basic setup.
        var lavalandMap = _map.CreateMap(out var lavalandMapId, runMapInit: false);
        var mapComp = EnsureComp<LavalandMapComponent>(lavalandMap);
        lavaland = (lavalandMap, mapComp);

        // If specified, force new seed
        seed ??= _random.Next();

        var lavalandPrototypeId = prototype.ID;

        PlanetBasicSetup(lavalandMap, prototype, seed.Value);

        // Ensure that it's paused
        _map.SetPaused(lavalandMapId, true);

        if (!SetupOutpost(lavalandMap, lavalandMapId, prototype.OutpostPath, out var outpost))
            return false;

        var loadBox = Box2.CentredAroundZero(new Vector2(prototype.RestrictedRange, prototype.RestrictedRange));

        mapComp.Outpost = outpost;
        mapComp.Seed = seed.Value;
        mapComp.PrototypeId = lavalandPrototypeId;
        mapComp.LoadArea = loadBox;

        // Setup Ruins.
        var pool = _proto.Index(prototype.RuinPool);
        SetupRuins(pool, lavaland.Value, preloader.Value);

        // Hide all grids from the mass scanner.
        foreach (var grid in _mapManager.GetAllGrids(lavalandMapId))
        {
            var flag = IFFFlags.HideLabel;

            /*#if DEBUG || TOOLS Uncomment me when GPS is done.
            flag = IFFFlags.HideLabel;
            #endif*/

            _shuttle.AddIFFFlag(grid, flag);
        }

        // Start!!1!!!
        _map.InitializeMap(lavalandMapId);

        // also preload the planet itself
        _biome.Preload(lavalandMap, Comp<BiomeComponent>(lavalandMap), loadBox);

        // Finally add destination
        var dest = AddComp<FTLDestinationComponent>(lavalandMap);
        dest.Whitelist = prototype.ShuttleWhitelist;

        return true;
    }

    private void PlanetBasicSetup(EntityUid lavalandMap, LavalandMapPrototype prototype, int seed)
    {
        // Name
        _metaData.SetEntityName(lavalandMap, Loc.GetString(prototype.Name));

        // Biomes
        _biome.EnsurePlanet(lavalandMap, _proto.Index(prototype.BiomePrototype), seed, mapLight: prototype.PlanetColor);

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

        // Atmos
        var air = prototype.Atmosphere;
        // copy into a new array since the yml deserialization discards the fixed length
        var moles = new float[Atmospherics.AdjustedNumberOfGases];
        air.CopyTo(moles, 0);

        var atmos = EnsureComp<MapAtmosphereComponent>(lavalandMap);
        _atmos.SetMapGasMixture(lavalandMap, new GasMixture(moles, prototype.Temperature), atmos);

        // Restricted Range
        var restricted = new RestrictedRangeComponent
        {
            Range = prototype.RestrictedRange,
        };
        AddComp(lavalandMap, restricted);

    }

    private bool SetupOutpost(EntityUid lavaland, MapId lavalandMapId, ResPath path, out EntityUid outpost)
    {
        outpost = EntityUid.Invalid;

        // Setup Outpost
        if (!_mapLoader.TryLoadGrid(lavalandMapId, path, out var outpostGrid))
        {
            Log.Error("Failed to load Lavaland outpost!");
            return false;
        }

        outpost = outpostGrid.Value;

        // Align outpost to planet
        _transform.SetCoordinates(outpost, new EntityCoordinates(lavaland, 0, 0));

        // Name it
        _metaData.SetEntityName(outpost, Loc.GetString("lavaland-planet-outpost"));
        var member = EnsureComp<LavalandMemberComponent>(outpost);
        member.SignalName = Loc.GetString("lavaland-planet-outpost");

        return true;
    }
}
