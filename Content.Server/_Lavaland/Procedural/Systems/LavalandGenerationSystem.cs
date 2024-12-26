using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Server._Lavaland.Procedural.Components;
using Content.Server._Lavaland.Procedural.Prototypes;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.GameTicking;
using Content.Server.Parallax;
using Content.Server.Shuttles.Systems;
using Content.Shared.Atmos;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Gravity;
using Content.Shared.Maps;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Salvage;
using Content.Shared.Shuttles.Components;
using Content.Shared.Whitelist;
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
    [ViewVariables]
    public List<LavalandMap>? LavalandMaps = [];

    [ViewVariables]
    private (EntityUid Uid, MapId Id)? _lavalandPreloader; // Global map for lavaland preloading

    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetConfigurationManager _config = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!; // used on debug
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly TileSystem _tileSystem = default!; // used on debug
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!; // used on NOT debug

    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<TransformComponent> _xformQuery;
    private EntityQuery<FixturesComponent> _fixtureQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PreGameMapLoad>(OnRoundStart);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnCleanup);

        _gridQuery = GetEntityQuery<MapGridComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
        _fixtureQuery = GetEntityQuery<FixturesComponent>();
    }

    private void OnRoundStart(PreGameMapLoad ev)
    {
        if (!_config.GetCVar(CCVars.LavalandEnabled))
        {
            LavalandMaps = null;
            return;
        }

        SetupPreloader();
        SetupLavaland();
    }

    private void OnCleanup(RoundRestartCleanupEvent ev)
    {
        ShutdownPreloader();
        LavalandMaps?.Clear();
    }

    private void SetupPreloader()
    {
        if (_lavalandPreloader != null &&
            !TerminatingOrDeleted(_lavalandPreloader.Value.Uid))
            return;

        var mapUid = _map.CreateMap(out var mapId, false);
        _metaData.SetEntityName(mapUid, "Lavaland Preloader Map");
        _map.SetPaused(mapId, true);
        _lavalandPreloader = (mapUid, mapId);
    }

    private void ShutdownPreloader()
    {
        if (_lavalandPreloader == null ||
            TerminatingOrDeleted(_lavalandPreloader.Value.Uid))
            return;

        _mapManager.DeleteMap(_lavalandPreloader.Value.Id);
        _lavalandPreloader = null;
    }

    public bool GetLavalands([NotNullWhen(true)] out List<LavalandMap>? maps)
    {
        maps = null;

        if (LavalandMaps == null)
            return false;

        maps = LavalandMaps;

        return true;
    }

    public bool SetupLavaland(int? seed = null, LavalandMapPrototype? prototype = null)
    {
        if (LavalandMaps == null)
            return false;

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

        foreach (var marker in prototype.OreLayers)
        {
            _biome.AddMarkerLayer(lavalandMap, biome, marker);
        }
        foreach (var marker in prototype.MobLayers)
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

        LavalandMaps!.Add(map);

        // Setup Ruins.
        var pool = _proto.Index(prototype.RuinPool);
        SetupRuins(pool, map);

        // Hide all grids from the mass scanner.
#if !DEBUG
        foreach (var grid in _mapManager.GetAllGrids(lavalandMapId))
        {
            _shuttle.AddIFFFlag(grid, IFFFlags.Hide);
        }
#endif
        // Start!!1!!!
        _mapManager.DoMapInitialize(lavalandMapId);
        _mapManager.SetMapPaused(lavalandMapId, false);

        // Finally add destination, only for Mining Shittles
        var dest = AddComp<FTLDestinationComponent>(lavalandMap);
        dest.Whitelist = new EntityWhitelist {Components = ["MiningShuttle"]};

        return true;
    }

    private void SetupRuins(LavalandRuinPoolPrototype pool, LavalandMap lavaland)
    {
        var random = new Random(lavaland.Seed);

        var boundary = GetOutpostBoundary(lavaland);
        if (boundary == null)
            return;

        // The LINQ shit is for filtering out all points that are inside the boundary.
        var coords = GetCoordinates(pool.RuinDistance, pool.MaxDistance);
        var ruinsBounds = CalculateRuinBounds(pool);

        List<LavalandRuinPrototype> hugeRuins = [];
        List<LavalandRuinPrototype> smallRuins = [];

        int i; // ruins stuff
        int j; // attemps for loading
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

        var randomCoords = coords.ToHashSet();

        if (hugeRuins.Count >= coords.Count)
        {
            Log.Error("Too many Huge ruins for Lavaland map to handle!");
            return;
        }

        // Try to load everything...
        var usedSpace = boundary.ToHashSet();

        // The first priority is for Huge ruins, they are required to be spawned.
        for (i = 0; i < hugeRuins.Count; i++)
        {
            var ruin = hugeRuins[i];
            if (!ruinsBounds.TryGetValue(ruin.ID, out var box))
                continue;

            const int attemps = 5;
            for (j = 0; j < attemps; j++)
            {
                if (LoadRuin(ruin, lavaland, box, random, ref usedSpace, ref randomCoords))
                    break;
            }
        }

        // Create a new list that excludes all already used spaces that intersect with big ruins.
        // Sweet optimization (another lag machine).
        var newCoords = randomCoords.ToHashSet();
        foreach (var usedBox in usedSpace)
        {
            var list = randomCoords.Where(coord => !usedBox.Contains(coord)).ToHashSet();
            newCoords = newCoords.Concat(list).ToHashSet();
        }

        if (smallRuins.Count >= newCoords.Count)
        {
            Log.Error("Too many Small ruins for Lavaland map to handle!");
            return;
        }

        // Go through all small ruins.
        for (i = 0; i < smallRuins.Count; i++)
        {
            var ruin = smallRuins[i];
            if (!ruinsBounds.TryGetValue(ruin.ID, out var box))
                continue;

            const int attemps = 3;
            for (j = 0; j < attemps; j++)
            {
                if (LoadRuin(ruin, lavaland, box, random, ref usedSpace, ref newCoords))
                    break;
            }
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
    private bool LoadRuin(
        LavalandRuinPrototype ruin,
        LavalandMap lavaland,
        List<Box2> ruinBox,
        Random random,
        ref HashSet<Box2> usedSpace,
        ref HashSet<Vector2> coords)
    {
        var coord = random.Pick(coords);

        // Why there's no method to move the Box2 around???
        var bounds = new List<Box2>();
        foreach (var box in ruinBox)
        {
            var v1 = box.BottomLeft + coord;
            var v2 = box.TopRight + coord;
            bounds.Add(new Box2(v1, v2));
        }

        // If any used boundary intersects with current boundary, return
        if ((from used in usedSpace from bound in bounds where bound.Intersects(used) select used).Any())
        {
            coords.Remove(coord);
            Log.Debug("Ruin can't be placed on it's coordinates, skipping spawn");
            return false;
        }

        var salvMap = _lavalandPreloader!.Value.Uid;
        var mapXform = Transform(salvMap);
        var gridsCount = _mapManager.GetAllGrids(lavaland.MapId).Count();

        // Try to load everything on a dummy map
        var opts = new MapLoadOptions
        {
            Offset = coord
        };

        if (!_mapLoader.TryLoad(mapXform.MapID, ruin.Path, out _, opts) || mapXform.ChildCount != 1)
        {
            Log.Error($"Failed to load ruin {ruin.ID} onto dummy map!");
            return false;
        }

        var mapChildren = mapXform.ChildEnumerator;

        // It worked, move it into position and cleanup values.
        while (mapChildren.MoveNext(out var mapChild))
        {
            var salvXForm = _xformQuery.GetComponent(mapChild);
            _transform.SetParent(mapChild, salvXForm, lavaland.Uid);
            _transform.SetCoordinates(mapChild, new EntityCoordinates(lavaland.Uid, salvXForm.Coordinates.Position.Rounded()));
            _metaData.SetEntityName(mapChild, ruin.Name);
            EnsureComp<LavalandRuinComponent>(mapChild);
        }

        // There should be more grids on Lavaland than before after re-parenting.
        if (_mapManager.GetAllGrids(lavaland.MapId).Count() <= gridsCount)
        {
            Log.Error("Failed to re-parent the grid from dummy map to Lavaland!");
            return false;
        }

#if DEBUG
        // Markup da area for debug purposes.
        var grid = Comp<MapGridComponent>(lavaland.Uid);
        var tiles = (from bound in bounds select _map.GetTilesIntersecting(lavaland.Uid, grid, bound, false));
        var sand = (ContentTileDefinition) _tileDefinitionManager["FloorAsteroidCoarseSand0"];

        // im sorry for making those programming war crimes
        foreach (var tile in tiles)
        {
            foreach (var til in tile)
            {
                _tileSystem.ReplaceTile(til, sand);
            }
        }
#endif

        usedSpace = usedSpace.Concat(bounds).ToHashSet();
        coords.Remove(coord);
        return true;
    }

    private Dictionary<ProtoId<LavalandRuinPrototype>, List<Box2>> CalculateRuinBounds(LavalandRuinPoolPrototype pool)
    {
        var ruinBounds = new Dictionary<ProtoId<LavalandRuinPrototype>, List<Box2>>();

        // All possible ruins for this pool
        var ruins = pool.SmallRuins.Keys.ToList().Concat(pool.HugeRuins.Keys).ToHashSet();

        foreach (var id in ruins)
        {
            var mapId = _lavalandPreloader!.Value.Id;
            var mapUid = _lavalandPreloader.Value.Uid;
            var dummyMapXform = Transform(mapUid);

            var proto = _proto.Index(id);
            var bounds = new List<Box2>();

            // Try to load everything on a dummy map
            var opts = new MapLoadOptions();

            if (!_mapLoader.TryLoad(mapId, proto.Path, out _, opts) || dummyMapXform.ChildCount == 0)
            {
                Log.Error($"Failed to load ruin {proto.ID} onto dummy map!");
                continue;
            }

            var mapChildren = dummyMapXform.ChildEnumerator;
            while (mapChildren.MoveNext(out var mapChild))
            {
                if (!_gridQuery.TryGetComponent(mapChild, out _) ||
                    !_fixtureQuery.TryGetComponent(mapChild, out var manager) ||
                    !_xformQuery.TryGetComponent(mapChild, out var xform) ||
                    xform.MapUid == null)
                    continue;

                var transform = _physics.GetRelativePhysicsTransform((mapChild, xform), xform.MapUid.Value);
                bounds = (from fixture in manager.Fixtures.Values where fixture.Hard select fixture.Shape.ComputeAABB(transform, 0).Rounded(0)).ToList();
                Del(mapChild); // We don't need it anymore
            }

            ruinBounds.Add(id, bounds);
        }

        return ruinBounds;
    }
}

public sealed class LavalandMap
{
    public EntityUid Uid;
    public MapId MapId = MapId.Nullspace;
    public ProtoId<LavalandMapPrototype>? PrototypeId;
    public int Seed;
    public EntityUid Outpost;

    public override string ToString()
    {
        var lavaland = $"Type: {PrototypeId} , MapID: {MapId} , MapUid: {Uid} , Seed: {Seed}";
        return lavaland;
    }
}
