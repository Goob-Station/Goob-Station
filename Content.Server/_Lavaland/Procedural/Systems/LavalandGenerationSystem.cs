using System.Linq;
using Content.Server._Lavaland.Procedural.Prototypes;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.GameTicking.Events;
using Content.Server.Parallax;
using Content.Shared.Atmos;
using Content.Shared.Gravity;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Shuttles.Components;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Lavaland.Procedural.Systems;

/// <summary>
/// Basic system to create Lavaland planet.
/// </summary>
public sealed class LavalandGenerationSystem : EntitySystem
{
    public EntityUid LavalandMap;
    public MapId LavalandMapId = MapId.Nullspace;
    public string? LavalandPrototypeId;
    public int Seed;

    public EntityUid LavalandOutpost;

    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly INetConfigurationManager _config = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        //if (_config.GetCVar())
        SetupLavaland();
    }

    private void SetupLavaland(int? seed = null, LavalandMapPrototype? prototype = null)
    {
        // Basic setup.
        LavalandMap = _map.CreateMap(out LavalandMapId, runMapInit: false);

        // If specified, force new seed or prototype
        seed ??= _random.Next();
        prototype ??= _random.Pick(_proto.EnumeratePrototypes<LavalandMapPrototype>().ToList());
        Seed = seed.Value;

        LavalandPrototypeId = prototype.ID;
        _metaData.SetEntityName(LavalandMap, prototype.Name);

        // Biomes
        _biome.EnsurePlanet(LavalandMap, _proto.Index(prototype.BiomePrototype), Seed, mapLight: prototype.PlanetColor);

        // Marker Layers
        var biome = EnsureComp<BiomeComponent>(LavalandMap);
        var oreLayers = prototype.OreLayers;
        foreach (var marker in oreLayers)
        {
            _biome.AddMarkerLayer(LavalandMap, biome, marker);
        }
        Dirty(LavalandMap, biome);

        // Gravity
        var gravity = EnsureComp<GravityComponent>(LavalandMap);
        gravity.Enabled = true;
        Dirty(LavalandMap, gravity);

        // Atmos
        var air = prototype.Atmosphere;
        // copy into a new array since the yml deserialization discards the fixed length
        var moles = new float[Atmospherics.AdjustedNumberOfGases];
        air.CopyTo(moles, 0);

        var atmos = EnsureComp<MapAtmosphereComponent>(LavalandMap);
        _atmos.SetMapGasMixture(LavalandMap, new GasMixture(moles, prototype.Temperature), atmos);

        _mapManager.SetMapPaused(LavalandMapId, true);

        // Setup Outpost
        var options = new MapLoadOptions();
        if (!_mapLoader.TryLoad(LavalandMapId, prototype.OutpostPath, out var outposts, options) || outposts.Count != 1)
        {
            Log.Error(outposts?.Count > 1
                ? $"Loading Outpost on lavaland map failed, {prototype.OutpostPath} is not saved as a grid."
                : $"Failed to spawn Outpost {prototype.OutpostPath} onto Lavaland map.");
            return;
        }

        // Setup Ruins
        // TODO generate ruins

        _mapManager.DoMapInitialize(LavalandMapId);
        _mapManager.SetMapPaused(LavalandMapId, false);
    }
}
