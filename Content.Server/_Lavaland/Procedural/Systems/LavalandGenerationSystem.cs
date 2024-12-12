using System.Linq;
using System.Numerics;
using Content.Server._Lavaland.Procedural.Components;
using Content.Server._Lavaland.Procedural.Prototypes;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.GameTicking.Events;
using Content.Server.Parallax;
using Content.Shared.Atmos;
using Content.Shared.Gravity;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Salvage;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
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
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

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

    public bool SetupLavaland(int? seed = null, LavalandMapPrototype? prototype = null)
    {
        if (LavalandMap.IsValid())
            return false;

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

        // Restricted Range
        var restricted = new RestrictedRangeComponent
        {
            Range = prototype.RestrictedRange,
        };
        AddComp(LavalandMap, restricted);

        // Setup Outpost
        var fixin = new Vector2(0.53125f, 0.390625f);
        var options = new MapLoadOptions
        {
            Offset = fixin,
        };
        if (!_mapLoader.TryLoad(LavalandMapId, prototype.OutpostPath, out var outposts, options) || outposts.Count != 1)
        {
            Log.Error(outposts?.Count > 1
                ? $"Loading Outpost on lavaland map failed, {prototype.OutpostPath} is not saved as a grid."
                : $"Failed to spawn Outpost {prototype.OutpostPath} onto Lavaland map.");
            return false;
        }

        // Get the outpost
        foreach (var grid in _mapManager.GetAllGrids(LavalandMapId))
        {
            if (!HasComp<LavalandOutpostComponent>(grid))
                continue;

            LavalandOutpost = grid;
            break;
        }

        // Setup Ruins
        var pool = _proto.Index(prototype.RuinPool);
        SetupRuins(pool);

        _mapManager.DoMapInitialize(LavalandMapId);
        _mapManager.SetMapPaused(LavalandMapId, false);
        return true;
    }

    private void SetupRuins(LavalandRuinPoolPrototype pool)
    {
        // TODO: THIS IS A FUCKING LAG MACHINE AAAAAAAAAAAAAAAAAAA
        var random = new Random(Seed);

        var boundary = GetOutpostBoundary();
        // The LINQ shit is to filter out all points that are inside the boundary.
        var coords = GetCoordinates(pool.RuinDistance, pool.MaxDistance)
            .Where(coordinate => boundary == null ||
            !boundary.Any(box => box.Contains(coordinate)))
            .ToList();

        List<LavalandRuinPrototype> ruins = [];

        foreach (var selectRuin in pool.Ruins)
        {
            var proto = _proto.Index(selectRuin.Key);
            for (ushort i = 0; i < selectRuin.Value; i++)
            {
                ruins.Add(proto);
            }
        }

        // No ruins no fun
        if (ruins.Count == 0)
            return;

        random.Shuffle(coords);
        random.Shuffle(ruins);

        var iterator = 0;
        while (iterator < ruins.Count - 1 && iterator < coords.Count - 1)
        {
            iterator++;

            var ruin = ruins[iterator];
            var coord = coords[iterator];
            // GAMBLING the coordinate a bit
            const int shift = 8;
            var shiftVector = new Vector2(random.Next(-shift, shift), random.Next(-shift, shift));
            coord += shiftVector;

            SpawnRuin(coord, ruin);
        }
    }

    private void SpawnRuin(Vector2 coordinate, LavalandRuinPrototype ruin)
    {
        var fixin = new Vector2(0.515625f, 0.5f);
        var options = new MapLoadOptions
        {
            Offset = coordinate + fixin,
        };
        if (!_mapLoader.TryLoad(LavalandMapId, ruin.Path, out var loaded, options) || loaded.Count != 1)
        {
            Log.Error(loaded?.Count > 1
                ? $"Loading Ruin on lavaland map failed, {ruin.Path} is not saved as a grid."
                : $"Failed to spawn Ruin {ruin.Path} onto Lavaland map.");
        }
        Log.Debug($"Loaded lavaland ruin {ruin.ID} on coordinates {coordinate.Rounded()}");
    }

    private List<Vector2> GetCoordinates(float distance, float maxDistance)
    {
        var coords = new List<Vector2>();
        var moveVector = new Vector2(maxDistance, maxDistance);

        while (moveVector.Y >= -maxDistance)
        {
            // i love writing shitcode
            // Moving like a snake through the entire map placing all dots onto its places.

            while (moveVector.X > -maxDistance)
            {
                coords.Add(moveVector);
                moveVector += new Vector2(-distance, 0);
            }

            coords.Add(moveVector);
            moveVector += new Vector2(0, -distance);

            while (moveVector.X < maxDistance)
            {
                coords.Add(moveVector);
                moveVector += new Vector2(distance, 0);
            }

            coords.Add(moveVector);
            moveVector += new Vector2(0, -distance);
        }

        return coords;
    }

    private List<Box2>? GetOutpostBoundary(FixturesComponent? manager = null, TransformComponent? xform = null)
    {
        var uid = LavalandOutpost;

        if (!Resolve(uid, ref manager, ref xform) || xform.MapUid != LavalandMap)
            return null;

        var aabbs = new List<Box2>(manager.Fixtures.Count);

        var transform = _physics.GetRelativePhysicsTransform((uid, xform), xform.MapUid.Value);
        foreach (var fixture in manager.Fixtures.Values)
        {
            if (!fixture.Hard)
                return null;

            var aabb = fixture.Shape.ComputeAABB(transform, 0);
            aabb = aabb.Enlarged(16f);
            aabbs.Add(aabb);
        }

        return aabbs;
    }
}
