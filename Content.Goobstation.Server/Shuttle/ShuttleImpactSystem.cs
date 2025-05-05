// ported from: monolith (Content.Server/Shuttles/Systems/ShuttleSystem.Impact.cs)
using Content.Server.Shuttles.Components;
using Content.Shared.Audio;
using Content.Shared.Buckle.Components;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Slippery;
using Content.Server.Stunnable;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Threading;
using System.Numerics;

namespace Content.Goobstation.Server.Shuttle.Impact;

public sealed partial class ShuttleImpactSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageSys = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly MapSystem _mapSys = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StunSystem _stuns = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;

    /// <summary>
    /// Minimum velocity difference between 2 bodies for a shuttle "impact" to occur.
    /// </summary>
    private const int MinimumImpactVelocity = 15;

    /// <summary>
    /// Kinetic energy required to dismantle a single tile
    /// </summary>
    private const float TileBreakEnergy = 10000;

    /// <summary>
    /// Kinetic energy required to spawn sparks
    /// </summary>
    private const float SparkEnergy = 14000;

    /// <summary>
    /// Maximum impact radius in tiles
    /// </summary>
    private const int MaxImpactRadius = 1;

    private readonly SoundCollectionSpecifier _shuttleImpactSound = new("ShuttleImpactSound");

    private void InitializeImpact()
    {
        SubscribeLocalEvent<ShuttleComponent, StartCollideEvent>(OnShuttleCollide);
    }

    /// <summary>
    /// Handles collision between two shuttles, applying impact damage and effects.
    /// Larger shuttles (by mass) will not experience impact effects when colliding with smaller shuttles.
    /// Smaller shuttles will still experience the full impact regardless.
    /// </summary>
    private void OnShuttleCollide(EntityUid uid, ShuttleComponent component, ref StartCollideEvent args)
    {
        if (!TryComp<MapGridComponent>(uid, out var ourGrid) ||
            !TryComp<MapGridComponent>(args.OtherEntity, out var otherGrid))
            return;

        // Skip impact processing if either grid has an anchor component
        // Goob - not real
        //if (HasComp<PreventGridAnchorChangesComponent>(uid) ||
        //    HasComp<ForceAnchorComponent>(uid) ||
        //    HasComp<PreventGridAnchorChangesComponent>(args.OtherEntity) ||
        //    HasComp<ForceAnchorComponent>(args.OtherEntity))
        //    return;

        var ourBody = args.OurBody;
        var otherBody = args.OtherBody;

        // TODO: Would also be nice to have a continuous sound for scraping.
        var ourXform = Transform(uid);

        if (ourXform.MapUid == null)
            return;

        var otherXform = Transform(args.OtherEntity);

        var ourPoint = Vector2.Transform(args.WorldPoint, _transform.GetInvWorldMatrix(ourXform));
        var otherPoint = Vector2.Transform(args.WorldPoint, _transform.GetInvWorldMatrix(otherXform));

        var ourVelocity = _physics.GetLinearVelocity(uid, ourPoint, ourBody, ourXform);
        var otherVelocity = _physics.GetLinearVelocity(args.OtherEntity, otherPoint, otherBody, otherXform);
        var jungleDiff = (ourVelocity - otherVelocity).Length();

        if (jungleDiff < MinimumImpactVelocity)
            return;

        var energy = ourBody.Mass * Math.Pow(jungleDiff, 2) / 2;
        var dir = (ourVelocity.Length() > otherVelocity.Length() ? ourVelocity : -otherVelocity).Normalized();

        // Calculate the impact radius based on energy, but capped at MaxImpactRadius
        var impactRadius = Math.Min(
            (int)Math.Ceiling(Math.Sqrt((double)energy / TileBreakEnergy)),
            MaxImpactRadius
        );

        // Convert the collision point directly to tile indices
        var ourTile = new Vector2i((int)Math.Floor(ourPoint.X / ourGrid.TileSize), (int)Math.Floor(ourPoint.Y / ourGrid.TileSize));
        var otherTile = new Vector2i((int)Math.Floor(otherPoint.X / otherGrid.TileSize), (int)Math.Floor(otherPoint.Y / otherGrid.TileSize));

        // Play impact sound
        var coordinates = new EntityCoordinates(ourXform.MapUid.Value, args.WorldPoint);

        // Compare masses to determine which shuttle should process the impact
        bool ourShuttleIsHeavier = ourBody.Mass > otherBody.Mass;
        bool otherShuttleIsHeavier = otherBody.Mass > ourBody.Mass;

        // Process impact zones sequentially to avoid race conditions
        try
        {
            // Only apply impact to our shuttle if it's not heavier
            if (!ourShuttleIsHeavier && Exists(uid) && HasComp<Robust.Shared.Physics.BroadphaseComponent>(uid))
            {
                ProcessImpactZone(uid, ourGrid, ourTile, (float)energy, -dir, impactRadius);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error processing impact zone for {ToPrettyString(uid)}: {ex}");
        }

        try
        {
            // Only apply impact to other shuttle if it's not heavier
            if (!otherShuttleIsHeavier && Exists(args.OtherEntity) && HasComp<Robust.Shared.Physics.BroadphaseComponent>(args.OtherEntity))
            {
                ProcessImpactZone(args.OtherEntity, otherGrid, otherTile, (float)energy, dir, impactRadius);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error processing impact zone for {ToPrettyString(args.OtherEntity)}: {ex}");
        }

        // Knockdown unbuckled entities on both grids
        try
        {
            // Only knockdown entities on our shuttle if it's not heavier
            if (!ourShuttleIsHeavier && Exists(uid))
            {
                KnockdownEntitiesOnGrid(uid);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error knocking down entities on grid {ToPrettyString(uid)}: {ex}");
        }

        try
        {
            // Only knockdown entities on other shuttle if it's not heavier
            if (!otherShuttleIsHeavier && Exists(args.OtherEntity))
            {
                KnockdownEntitiesOnGrid(args.OtherEntity);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error knocking down entities on grid {ToPrettyString(args.OtherEntity)}: {ex}");
        }
    }

    /// <summary>
    /// Knocks down all unbuckled entities on the specified grid.
    /// </summary>
    private void KnockdownEntitiesOnGrid(EntityUid gridUid)
    {
        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        // Find all entities on the grid
        var buckleQuery = GetEntityQuery<BuckleComponent>();
        var noSlipQuery = GetEntityQuery<NoSlipComponent>();
        var magbootsQuery = GetEntityQuery<MagbootsComponent>();
        var itemToggleQuery = GetEntityQuery<ItemToggleComponent>();
        var knockdownTime = TimeSpan.FromSeconds(5);

        // Get all entities with MobState component on the grid
        var query = EntityQueryEnumerator<MobStateComponent, TransformComponent>();

        // Create a collection to batch knockdown operations
        var entitiesToKnockdown = new List<EntityUid>();

        while (query.MoveNext(out var uid, out var mobState, out var xform))
        {
            // Skip entities not on this grid
            if (xform.GridUid != gridUid || !Exists(uid))
                continue;

            // If entity has a buckle component and is buckled, skip it
            if (buckleQuery.TryGetComponent(uid, out var buckle) && buckle.Buckled)
                continue;

            // Skip if the entity directly has NoSlip component
            if (noSlipQuery.HasComponent(uid))
                continue;

            // Check if the entity is wearing magboots
            bool hasMagboots = false;

            // Check if they're wearing shoes with NoSlip component or activated magboots
            if (_inventorySystem.TryGetSlotEntity(uid, "shoes", out var shoes) && Exists(shoes))
            {
                // If shoes have NoSlip component
                if (noSlipQuery.HasComponent(shoes))
                {
                    hasMagboots = true;
                }
                // If shoes are magboots and they are activated
                else if (magbootsQuery.HasComponent(shoes) &&
                         itemToggleQuery.TryGetComponent(shoes, out var toggle) &&
                         toggle.Activated)
                {
                    hasMagboots = true;
                }
            }

            if (hasMagboots)
                continue;

            // Add entity to knockdown batch
            entitiesToKnockdown.Add(uid);
        }

        // Apply knockdowns sequentially for safety
        foreach (var uid in entitiesToKnockdown)
        {
            if (Exists(uid))
            {
                try
                {
                    _stuns.TryKnockdown(uid, knockdownTime, true);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error applying knockdown to {ToPrettyString(uid)}: {ex}");
                }
            }
        }
    }

    /// <summary>
    /// Structure to hold impact tile processing data
    /// </summary>
    private readonly struct ImpactTileData
    {
        public readonly Vector2i Tile;
        public readonly float Energy;
        public readonly float DistanceFactor;
        public readonly Vector2 ThrowDirection;

        public ImpactTileData(Vector2i tile, float energy, float distanceFactor, Vector2 throwDirection)
        {
            Tile = tile;
            Energy = energy;
            DistanceFactor = distanceFactor;
            ThrowDirection = throwDirection;
        }
    }

    /// <summary>
    /// Processes a zone of tiles around the impact point
    /// </summary>
    private void ProcessImpactZone(EntityUid uid, MapGridComponent grid, Vector2i centerTile, float energy, Vector2 dir, int radius)
    {
        // Skip processing if the grid has an anchor component
        if (!Exists(uid) ||
            // Goob - not real
            //HasComp<PreventGridAnchorChangesComponent>(uid) ||
            //HasComp<ForceAnchorComponent>(uid) ||
            !HasComp<Robust.Shared.Physics.BroadphaseComponent>(uid))
            return;

        // Create a list of all tiles to process
        var tilesToProcess = new List<ImpactTileData>();

        // Pre-calculate all tiles that need processing
        for (var x = -radius; x <= radius; x++)
        {
            for (var y = -radius; y <= radius; y++)
            {
                // Skip tiles too far from impact center (creating a rough circle)
                if (x*x + y*y > radius*radius)
                    continue;

                Vector2i tile = new Vector2i(centerTile.X + x, centerTile.Y + y);

                // Calculate distance-based energy falloff
                float distanceFactor = 1.0f - (float)Math.Sqrt(x*x + y*y) / (radius + 1);
                float tileEnergy = energy * distanceFactor;

                tilesToProcess.Add(new ImpactTileData(tile, tileEnergy, distanceFactor, dir));
            }
        }

        // Create common damage specification
        DamageSpecifier damageTemplate = new();
        damageTemplate.DamageDict = new() { { "Blunt", energy } };

        // Process tiles sequentially for safety
        var brokenTiles = new List<Vector2i>();
        var sparkTiles = new List<Vector2i>();

        ProcessTileBatch(uid, grid, tilesToProcess, 0, tilesToProcess.Count, damageTemplate, brokenTiles, sparkTiles);

        // Only proceed with visual effects if the entity still exists
        if (Exists(uid))
        {
            ProcessBrokenTilesAndSparks(uid, grid, brokenTiles, sparkTiles);
        }
    }

    /// <summary>
    /// Process a batch of tiles from the impact zone
    /// </summary>
    private void ProcessTileBatch<T>(
        EntityUid uid,
        MapGridComponent grid,
        List<ImpactTileData> tilesToProcess,
        int startIndex,
        int endIndex,
        DamageSpecifier damageTemplate,
        T brokenTiles,
        T sparkTiles) where T : ICollection<Vector2i>
    {
        for (var i = startIndex; i < endIndex; i++)
        {
            var tileData = tilesToProcess[i];

            try
            {
                if (!Exists(uid) || !HasComp<Robust.Shared.Physics.BroadphaseComponent>(uid))
                    continue;

                // Process entities on this tile
                var entitiesOnTile = new HashSet<EntityUid>();

                try
                {
                    _lookup.GetLocalEntitiesIntersecting(uid, tileData.Tile, entitiesOnTile, gridComp: grid);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error getting entities on tile {tileData.Tile} for grid {ToPrettyString(uid)}: {ex}");
                    continue;
                }

                foreach (var localUid in entitiesOnTile)
                {
                    if (!Exists(localUid))
                        continue;

                    try
                    {
                        // Apply damage scaled by distance but capped to prevent gibbing
                        var scaledDamage = tileData.Energy;
                        var damageSpec = new DamageSpecifier(damageTemplate)
                        {
                            DamageDict = { ["Blunt"] = scaledDamage }
                        };

                        _damageSys.TryChangeDamage(localUid, damageSpec);

                        // Handle anchoring and throwing
                        if (TryComp<TransformComponent>(localUid, out var form))
                        {
                            if (!form.Anchored)
                                _transform.Unanchor(localUid, form);

                            if (Exists(localUid))
                                _throwing.TryThrow(localUid, tileData.ThrowDirection * tileData.DistanceFactor);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error processing impact on entity {ToPrettyString(localUid)}: {ex}");
                    }
                }

                // Mark tiles for breaking/effects
                if (tileData.Energy > TileBreakEnergy)
                {
                    brokenTiles.Add(tileData.Tile);
                }

                // Mark tiles for spark effects
                if (tileData.Energy > SparkEnergy && tileData.DistanceFactor > 0.7f)
                {
                    sparkTiles.Add(tileData.Tile);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error processing tile {tileData.Tile} during impact: {ex}");
            }
        }
    }

    /// <summary>
    /// Process visual effects and tile breaking after entity processing
    /// </summary>
    private void ProcessBrokenTilesAndSparks<TCollection>(
        EntityUid uid,
        MapGridComponent grid,
        TCollection brokenTiles,
        TCollection sparkTiles) where TCollection : IEnumerable<Vector2i>
    {
        if (!Exists(uid))
            return;

        // Break tiles
        foreach (var tile in brokenTiles)
        {
            try
            {
                if (Exists(uid))
                    _mapSys.SetTile(new Entity<MapGridComponent>(uid, grid), tile, Tile.Empty);
            }
            catch (Exception ex)
            {
                Log.Error($"Error breaking tile {tile} on grid {ToPrettyString(uid)}: {ex}");
            }
        }

        // Spawn spark effects
        foreach (var tile in sparkTiles)
        {
            try
            {
                if (!Exists(uid))
                    continue;

                var coords = grid.GridTileToLocal(tile);

                // Validate the coordinates before spawning
                var mapId = coords.GetMapId(EntityManager);
                if (mapId == MapId.Nullspace)
                    continue;

                if (!_mapManager.MapExists(mapId))
                    continue;

                var mapPos = coords.ToMap(EntityManager, _transform);
                if (mapPos.MapId == MapId.Nullspace)
                    continue;

                Spawn("EffectSparks", coords);
            }
            catch (Exception ex)
            {
                Log.Error($"Error spawning sparks at tile {tile} on grid {ToPrettyString(uid)}: {ex}");
            }
        }
    }
}
