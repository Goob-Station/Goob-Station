using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.GameTicking;
using Content.Server.Parallax;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared._Lavaland.Procedural.Prototypes;
using Content.Shared.Atmos;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Gravity;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Salvage;
using Content.Shared.Shuttles.Components;
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
public sealed class LavalandPlanetSystem : EntitySystem
{
    [ViewVariables]
    private (EntityUid Uid, MapId Id)? _lavalandPreloader; // Global map for lavaland preloading

    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetConfigurationManager _config = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<TransformComponent> _xformQuery;
    private EntityQuery<FixturesComponent> _fixtureQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PostGameMapLoad>(OnPreloadStart);
        SubscribeLocalEvent<RoundStartAttemptEvent>(OnRoundStart);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnCleanup);

        _gridQuery = GetEntityQuery<MapGridComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
        _fixtureQuery = GetEntityQuery<FixturesComponent>();
    }

    private void OnPreloadStart(PostGameMapLoad ev)
    {
        if (!_config.GetCVar(CCVars.LavalandEnabled))
        {
            return;
        }

        SetupLavalands();
    }

    private void OnRoundStart(RoundStartAttemptEvent ev)
    {
        if (!_config.GetCVar(CCVars.LavalandEnabled))
        {
            return;
        }

        var lavalands = GetLavalands();
        if (lavalands.Count == 0)
            return;

        var defaultStation = _station.GetStationInMap(_ticker.DefaultMap);
        if (defaultStation == null)
            return;

        foreach (var lavaland in lavalands)
        {
            // Add all outposts as a new station grid member
            _station.AddGridToStation(defaultStation.Value, lavaland.Comp.Outpost);
        }
    }

    private void OnCleanup(RoundRestartCleanupEvent ev)
    {
        ShutdownPreloader();
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

    public List<Entity<LavalandMapComponent>> GetLavalands()
    {
        var lavalandsQuery = EntityQueryEnumerator<LavalandMapComponent>();
        var lavalands = new List<Entity<LavalandMapComponent>>();
        while (lavalandsQuery.MoveNext(out var uid, out var comp))
        {
            lavalands.Add((uid, comp));
        }

        return lavalands;
    }

    /// <summary>
    /// Setup ALL instances of LavalandMapPrototype.
    /// </summary>
    public void SetupLavalands()
    {
        foreach (var lavaland in _proto.EnumeratePrototypes<LavalandMapPrototype>())
        {
            if (!SetupLavalandPlanet(out _, lavaland))
            {
                Log.Error($"Failed to load lavaland planet: {lavaland.ID}");
            }
        }
    }

    public bool SetupLavalandPlanet(out Entity<LavalandMapComponent>? lavaland,  LavalandMapPrototype prototype, int? seed = null)
    {
        if (_lavalandPreloader == null)
            SetupPreloader();

        // Basic setup.
        var lavalandMap = _map.CreateMap(out var lavalandMapId, runMapInit: false);
        var mapComp = EnsureComp<LavalandMapComponent>(lavalandMap);
        lavaland = (lavalandMap, mapComp);

        // If specified, force new seed
        seed ??= _random.Next();

        var lavalandPrototypeId = prototype.ID;

        PlanetBasicSetup(lavalandMap, prototype, seed.Value);

        _mapManager.SetMapPaused(lavalandMapId, true);

        if (!SetupOutpost(lavalandMap, lavalandMapId, prototype.OutpostPath, out var outpost))
            return false;

        var loadBox = Box2.CentredAroundZero(new Vector2(prototype.RestrictedRange, prototype.RestrictedRange));

        mapComp.Outpost = outpost;
        mapComp.Seed = seed.Value;
        mapComp.PrototypeId = lavalandPrototypeId;
        mapComp.LoadArea = loadBox;

        // Setup Ruins.
        var pool = _proto.Index(prototype.RuinPool);
        SetupRuins(pool, lavaland.Value);

        // Hide all grids from the mass scanner.
        foreach (var grid in _mapManager.GetAllGrids(lavalandMapId))
        {
            var flag = IFFFlags.Hide;

            #if DEBUG || TOOLS
            flag = IFFFlags.HideLabel;
            #endif

            _shuttle.AddIFFFlag(grid, flag);
        }

        // Start!!1!!!
        _mapManager.DoMapInitialize(lavalandMapId);
        _mapManager.SetMapPaused(lavalandMapId, false);

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

    private bool SetupOutpost(EntityUid lavaland, MapId lavalandMapId, string path, out EntityUid outpost)
    {
        outpost = EntityUid.Invalid;

        // Setup Outpost
        if (!_mapLoader.TryLoad(lavalandMapId, path, out var outposts) || outposts.Count != 1)
        {
            Log.Error(outposts?.Count > 1
                ? $"Loading Outpost on lavaland map failed, {path} is not saved as a grid."
                : $"Failed to spawn Outpost {path} onto Lavaland map.");
            return false;
        }

        // Get the outpost.
        foreach (var grid in _mapManager.GetAllGrids(lavalandMapId))
        {
            if (!HasComp<LavalandStationComponent>(grid))
                continue;

            outpost = grid;
            break;
        }

        if (TerminatingOrDeleted(outpost))
        {
            Log.Error("Lavaland outpost was loaded, but doesn't exist! (Maybe you forgot to add LavalandStationComponent?)");
            return false;
        }

        // Align outpost to planet
        _transform.SetCoordinates(outpost, new EntityCoordinates(lavaland, 0, 0));
        
        // Name it
        _metaData.SetEntityName(outpost, Loc.GetString("lavaland-planet-outpost"));
        var member = EnsureComp<LavalandMemberComponent>(outpost);
        member.SignalName = Loc.GetString("lavaland-planet-outpost");

        // Add outpost as a new station grid member (if it's in round)
        var defaultStation = _station.GetStationInMap(_ticker.DefaultMap);
        if (defaultStation != null && _ticker.RunLevel == GameRunLevel.InRound)
            _station.AddGridToStation(defaultStation.Value, outpost);

        return true;
    }

    private void SetupRuins(LavalandRuinPoolPrototype pool, Entity<LavalandMapComponent> lavaland)
    {
        var random = new Random(lavaland.Comp.Seed);

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

        foreach (var selectRuin in pool.SmallRuins)
        {
            var proto = _proto.Index(selectRuin.Key);
            for (i = 0; i < selectRuin.Value; i++)
            {
                smallRuins.Add(proto);
            }
        }

        // No ruins no fun
        if (hugeRuins.Count == 0 && smallRuins.Count == 0)
            return;

        random.Shuffle(coords);
        hugeRuins.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        smallRuins.Sort((x, y) => x.Priority.CompareTo(y.Priority));

        var randomCoords = coords.ToHashSet();
        var spawnSmallRuins = true;
        // Cut off ruins if there's not enough places for them
        if (hugeRuins.Count >= randomCoords.Count)
        {
            hugeRuins.RemoveRange(randomCoords.Count - 1, hugeRuins.Count - randomCoords.Count + 1);
            spawnSmallRuins = false;
        }

        // Try to load everything...
        var usedSpace = boundary.ToHashSet();

        // The first priority is for Huge ruins, they are required to be spawned.
        for (i = 0; i < hugeRuins.Count; i++)
        {
            var ruin = hugeRuins[i];
            if (!ruinsBounds.TryGetValue(ruin.ID, out var box))
                continue;

            for (j = 0; j < ruin.SpawnAttemps; j++)
            {
                if (!LoadRuin(ruin, lavaland, box, random, ref usedSpace, ref randomCoords, out var spawned))
                    continue;

                var member = EnsureComp<LavalandMemberComponent>(spawned.Value);
                member.SignalName = Loc.GetString(ruin.Name);
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

        if (newCoords.Count == 0 || !spawnSmallRuins)
            return;

        if (smallRuins.Count >= newCoords.Count)
        {
            smallRuins.RemoveRange(newCoords.Count, smallRuins.Count - newCoords.Count);
        }

        // Go through all small ruins.
        for (i = 0; i < smallRuins.Count; i++)
        {
            var ruin = smallRuins[i];
            if (!ruinsBounds.TryGetValue(ruin.ID, out var box))
                continue;

            for (j = 0; j < ruin.SpawnAttemps; j++)
            {
                if (LoadRuin(ruin, lavaland, box, random, ref usedSpace, ref newCoords, out var spawned))
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

    private List<Box2>? GetOutpostBoundary(Entity<LavalandMapComponent> lavaland, FixturesComponent? manager = null, TransformComponent? xform = null)
    {
        var uid = lavaland.Comp.Outpost;

        if (!Resolve(uid, ref manager, ref xform) || xform.MapUid != lavaland)
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

    private bool LoadRuin(
        LavalandRuinPrototype ruin,
        Entity<LavalandMapComponent> lavaland,
        List<Box2> ruinBox,
        Random random,
        ref HashSet<Box2> usedSpace,
        ref HashSet<Vector2> coords,
        [NotNullWhen(true)] out EntityUid? spawned)
    {
        spawned = null;
        if (coords.Count == 0)
            return false;

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
            _transform.SetParent(mapChild, salvXForm, lavaland);
            _transform.SetCoordinates(mapChild, new EntityCoordinates(lavaland, salvXForm.Coordinates.Position.Rounded()));
            _metaData.SetEntityName(mapChild, Loc.GetString(ruin.Name));
            spawned = mapChild;
        }

        if (spawned == null)
            return false;

        usedSpace = usedSpace.Concat(bounds).ToHashSet();
        coords.Remove(coord);
        return true;
    }

    private Dictionary<ProtoId<LavalandRuinPrototype>, List<Box2>> CalculateRuinBounds(LavalandRuinPoolPrototype pool)
    {
        var ruinBounds = new Dictionary<ProtoId<LavalandRuinPrototype>, List<Box2>>();

        if (_lavalandPreloader == null || TerminatingOrDeleted(_lavalandPreloader.Value.Uid))
        {
            SetupPreloader();
        }

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
