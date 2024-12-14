using System.Linq;
using System.Numerics;
using Content.Server._Lavaland.Procedural.Components;
using Content.Server._Lavaland.Procedural.Prototypes;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Parallax;
using Content.Server.Shuttles.Systems;
using Content.Shared.Atmos;
using Content.Shared.Gravity;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Salvage;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
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
    public List<LavalandMap> LavalandMaps = [];

    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly INetConfigurationManager _config = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PreGameMapLoad>(OnRoundStart);

        _gridQuery = GetEntityQuery<MapGridComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
    }

    private void OnRoundStart(PreGameMapLoad ev)
    {
        //if (_config.GetCVar())
        SetupLavaland();
    }

    public bool SetupLavaland(int? seed = null, LavalandMapPrototype? prototype = null)
    {
        // Basic setup.
        var lavalandMap = _map.CreateMap(out var lavalandMapId, runMapInit: false);

        // If specified, force new seed or prototype
        seed ??= _random.Next();
        prototype ??= _random.Pick(_proto.EnumeratePrototypes<LavalandMapPrototype>().ToList());
        var lavalandSeed = seed.Value;

        var lavalandPrototypeId = prototype.ID;
        _metaData.SetEntityName(lavalandMap, prototype.Name);

        // Biomes
        _biome.EnsurePlanet(lavalandMap, _proto.Index(prototype.BiomePrototype), lavalandSeed, mapLight: prototype.PlanetColor);

        // Marker Layers
        var biome = EnsureComp<BiomeComponent>(lavalandMap);
        var oreLayers = prototype.OreLayers;
        foreach (var marker in oreLayers)
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

        _mapManager.SetMapPaused(lavalandMapId, true);

        // Restricted Range
        var restricted = new RestrictedRangeComponent
        {
            Range = prototype.RestrictedRange,
        };
        AddComp(lavalandMap, restricted);

        // Setup Outpost
        var fixin = new Vector2(0.53125f, 0.390625f);
        var options = new MapLoadOptions
        {
            Offset = fixin,
        };
        if (!_mapLoader.TryLoad(lavalandMapId, prototype.OutpostPath, out var outposts, options) || outposts.Count != 1)
        {
            Log.Error(outposts?.Count > 1
                ? $"Loading Outpost on lavaland map failed, {prototype.OutpostPath} is not saved as a grid."
                : $"Failed to spawn Outpost {prototype.OutpostPath} onto Lavaland map.");
            return false;
        }

        // Get the outpost.
        var outpost = EntityUid.Invalid;
        foreach (var grid in _mapManager.GetAllGrids(lavalandMapId))
        {
            if (!HasComp<LavalandOutpostComponent>(grid))
                continue;

            outpost = grid;
            break;
        }

        if (TerminatingOrDeleted(outpost))
            return false;

        var map = new LavalandMap
        {
            MapId = lavalandMapId,
            PrototypeId = lavalandPrototypeId,
            Seed = lavalandSeed,
            Uid = lavalandMap,
            Outpost = outpost,
        };
        LavalandMaps.Add(map);

        // Setup Ruins.
        var pool = _proto.Index(prototype.RuinPool);
        SetupRuins(pool, map);

        // Hide all grids from the mass scanner.
        /*foreach (var grid in _mapManager.GetAllGrids(lavalandMapId))
        {
            _shuttle.AddIFFFlag(grid, IFFFlags.Hide);
        }*/

        // Start!!1!!!
        _mapManager.DoMapInitialize(lavalandMapId);
        _mapManager.SetMapPaused(lavalandMapId, false);
        return true;
    }

    private void SetupRuins(LavalandRuinPoolPrototype pool, LavalandMap lavaland)
    {
        // TODO: THIS IS A FUCKING LAG MACHINE HELP ME AAAAAAAAAAAAAAAAAAAAAAA

        var random = new Random(lavaland.Seed);

        var boundary = GetOutpostBoundary(lavaland);
        // The LINQ shit is for filtering out all points that are inside the boundary.
        var coords = GetCoordinates(pool.RuinDistance, pool.MaxDistance)
            .Where(coordinate => boundary == null ||
            !boundary.Any(box => box.Contains(coordinate)))
            .Where(cordinate => cordinate
            .IsShorterThan(CompOrNull<RestrictedRangeComponent>(lavaland.Uid)?.Range ?? float.MaxValue))
            .ToList();

        List<LavalandRuinPrototype> hugeRuins = [];
        List<LavalandRuinPrototype> smallRuins = [];

        int i; // ruins stuff
        int k; // coords stuff
        foreach (var selectRuin in pool.HugeRuins)
        {
            var proto = _proto.Index(selectRuin.Key);
            for (i = 0; i < selectRuin.Value; i++)
            {
                hugeRuins.Add(proto);
            }
        }

        // No ruins no fun
        if (hugeRuins.Count == 0)
            return;

        foreach (var selectRuin in pool.SmallRuins)
        {
            var proto = _proto.Index(selectRuin.Key);
            for (i = 0; i < selectRuin.Value; i++)
            {
                smallRuins.Add(proto);
            }
        }

        random.Shuffle(coords);
        random.Shuffle(smallRuins);
        random.Shuffle(hugeRuins);

        // Try to load everything...
        var usedSpace = new List<Box2>();

        // The first priority is for Huge ruins, they are required to be spawned.
        // God forgive me for using so much one-lettered variables.
        for (i = 0, k = 0; i < hugeRuins.Count && k < coords.Count; i++, k++)
        {
            var ruin = hugeRuins[i];
            var coord = coords[k];
            const int attemps = 3;

            for (var j = 0; j < attemps && !LoadRuin(coord, ruin, lavaland, ref usedSpace); j++)
            {
                k++; // Move through coordinates until we'll be able to find unused space
                coord = coords[k];
            }
        }

        // Create a new list that excludes all already used spaces that intersect with big ruins.
        // Sweet optimization (another lag machine).
        var newCoords = coords.ToList();
        foreach (var usedBox in usedSpace)
        {
            var list = coords.Where(coord => !usedBox.Contains(coord)).ToList();
            newCoords = newCoords.Concat(list).ToList();
        }

        // Go through all small ruins. We don't care if they are failed to spawn.
        for (i = 0; i < smallRuins.Count && i < coords.Count; i++)
        {
            var ruin = smallRuins[i];
            var coord = newCoords[i];
            LoadRuin(coord, ruin, lavaland, ref usedSpace);
        }
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

    private List<Box2>? GetOutpostBoundary(LavalandMap lavaland, FixturesComponent? manager = null, TransformComponent? xform = null)
    {
        var uid = lavaland.Outpost;

        if (!Resolve(uid, ref manager, ref xform) || xform.MapUid != lavaland.Uid)
            return null;

        var aabbs = new List<Box2>(manager.Fixtures.Count);

        var transform = _physics.GetRelativePhysicsTransform((uid, xform), xform.MapUid.Value);
        foreach (var fixture in manager.Fixtures.Values)
        {
            if (!fixture.Hard)
                return null;

            var aabb = fixture.Shape.ComputeAABB(transform, 0);
            aabb = aabb.Enlarged(8f);
            aabbs.Add(aabb);
        }

        return aabbs;
    }

    // TODO: make this as CPU job
    private bool LoadRuin(Vector2 coord, LavalandRuinPrototype ruin, LavalandMap lavaland, ref List<Box2> usedSpace)
    {
        var salvMap = _map.CreateMap();
        var dummyMapXform = Transform(salvMap);

        // Try to load everything on a dummy map
        if (ruin.DoPatch)
        {
            // TODO: do da black magic like in dungeon generation code
        }
        else
        {
            var opts = new MapLoadOptions
            {
                Offset = coord
            };

            if (!_mapLoader.TryLoad(dummyMapXform.MapID, ruin.Path, out _, opts))
            {
                Log.Error($"Failed to load ruin {ruin.ID} onto dummy map!");
                _mapManager.DeleteMap(dummyMapXform.MapID);
                return false;
            }
        }

        Box2? bounds = null;

        if (dummyMapXform.ChildCount == 0)
        {
            _mapManager.DeleteMap(dummyMapXform.MapID);
            return false;
        }

        var mapChildren = dummyMapXform.ChildEnumerator;

        while (mapChildren.MoveNext(out var mapChild))
        {
            if (!_gridQuery.TryGetComponent(mapChild, out var childGrid))
                continue;

            var childAABB = _transform.GetWorldMatrix(mapChild).TransformBox(childGrid.LocalAABB);
            bounds = bounds?.Union(childAABB) ?? childAABB;
            bounds = bounds.Value.Rounded(1);
        }

        if (bounds == null)
        {
            _mapManager.DeleteMap(dummyMapXform.MapID);
            return false;
        }

        var mapId = lavaland.MapId;

        if (!TryGetSalvagePlacementLocation(coord, mapId, bounds.Value, usedSpace, out var spawnLocation))
        {
            Log.Debug("Ruin can't be placed on it's coordinates, skipping spawn");
            _mapManager.DeleteMap(dummyMapXform.MapID);
            return false;
        }

        // I have no idea if we want to return on failure or not
        // but I assume trying to set the parent with a null value wouldn't have worked out anyways
        if (!_map.TryGetMap(spawnLocation.MapId, out var spawnUid))
            return false;

        mapChildren = dummyMapXform.ChildEnumerator;

        // It worked, move it into position and cleanup values.
        while (mapChildren.MoveNext(out var mapChild))
        {
            var salvXForm = _xformQuery.GetComponent(mapChild);
            var localPos = salvXForm.LocalPosition;

            _transform.SetParent(mapChild, salvXForm, spawnUid.Value);
            _transform.SetWorldPositionRotation(mapChild, (spawnLocation.Position + localPos).Rounded(), 0, salvXForm);
        }

        usedSpace.Add(bounds.Value);
        _mapManager.DeleteMap(dummyMapXform.MapID);
        return true;
    }

    private bool TryGetSalvagePlacementLocation(Vector2 tryCoords, MapId mapId, Box2 bounds, List<Box2> usedSpace, out MapCoordinates coords)
    {
        var finalCoords = new MapCoordinates(tryCoords, mapId);

        if (usedSpace.Any(used => bounds.Intersects(used)))
        {
            coords = MapCoordinates.Nullspace;
            return false;
        }

        coords = finalCoords;
        return true;
    }
}

public sealed class LavalandMap
{
    public EntityUid Uid;
    public MapId MapId = MapId.Nullspace;
    public ProtoId<LavalandMapPrototype>? PrototypeId;
    public int Seed;
    public EntityUid Outpost;
}
