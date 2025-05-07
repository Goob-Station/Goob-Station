// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// ported from: monolith (Content.Server/Shuttles/Systems/ShuttleSystem.Impact.cs)
using Content.Goobstation.Common.CCVar;
using Content.Server.Shuttles.Components;
using Content.Shared.Audio;
using Content.Shared.Buckle.Components;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Slippery;
using Content.Server.Stunnable;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Threading;
using System.Numerics;

namespace Content.Goobstation.Server.Shuttle.Impact;

public sealed partial class ShuttleImpactSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageSys = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
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
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;

    /// <summary>
    /// Minimum velocity difference between 2 bodies for a shuttle "impact" to occur.
    /// </summary>
    private float MinimumImpactVelocity;

    /// <summary>
    /// Multiplier of Kinetic energy required to dismantle a single tile in relation to its mass
    /// </summary>
    private float TileBreakEnergyMultiplier;

    /// <summary>
    /// Multiplier of damage done to entities on colliding areas
    /// </summary>
    private float DamageMultiplier;

    /// <summary>
    /// Kinetic energy required to spawn sparks
    /// </summary>
    private float SparkEnergy;

    /// <summary>
    /// Area to consider for impact calculations
    /// </summary>
    private float ImpactRadius;

    /// <summary>
    /// How much to slowdown per energy spent on impact
    /// </summary>
    private float ImpactSlowdown;

    private const float PlatingMass = 800f;

    private readonly SoundCollectionSpecifier _shuttleImpactSound = new("ShuttleImpactSound");

    public override void Initialize()
    {
        SubscribeLocalEvent<ShuttleComponent, StartCollideEvent>(OnShuttleCollide);

        Subs.CVar(_cfg, GoobCVars.MinimumImpactVelocity, value => MinimumImpactVelocity = value, true);
        Subs.CVar(_cfg, GoobCVars.TileBreakEnergyMultiplier, value => TileBreakEnergyMultiplier = value, true);
        Subs.CVar(_cfg, GoobCVars.ImpactDamageMultiplier, value => DamageMultiplier = value, true);
        Subs.CVar(_cfg, GoobCVars.SparkEnergy, value => SparkEnergy = value, true);
        Subs.CVar(_cfg, GoobCVars.ImpactRadius, value => ImpactRadius = value, true);
        Subs.CVar(_cfg, GoobCVars.ImpactSlowdown, value => ImpactSlowdown = value, true);
    }

    /// <summary>
    /// Handles collision between two shuttles, applying impact damage and effects.
    /// </summary>
    private void OnShuttleCollide(EntityUid uid, ShuttleComponent component, ref StartCollideEvent args)
    {
        if (!TryComp<MapGridComponent>(uid, out var ourGrid) ||
            !TryComp<MapGridComponent>(args.OtherEntity, out var otherGrid))
            return;

        var ourBody = args.OurBody;
        var otherBody = args.OtherBody;

        // TODO: Would also be nice to have a continuous sound for scraping.
        var ourXform = Transform(uid);
        var otherXform = Transform(args.OtherEntity);

        var ourPoint = _transform.ToCoordinates(args.OurEntity, new MapCoordinates(args.WorldPoint, ourXform.MapID));
        var otherPoint = _transform.ToCoordinates(args.OtherEntity, new MapCoordinates(args.WorldPoint, otherXform.MapID));

        bool evil = false;

        // for whatever reason collisions decide to go schizo sometimes and "collide" at some apparently random point
        if (!OnOrNearGrid((uid, ourGrid), ourPoint))
            evil = true;

        if (!evil && !OnOrNearGrid((args.OtherEntity, otherGrid), otherPoint))
            evil = true;

        var point = args.WorldPoint;

        // engine has provided a WorldPoint in the middle of nowhere, try workaround
        if (evil)
        {
            var contacts = _physics.GetContacts(uid);
            var coord = new Vector2(0, 0);
            while (contacts.MoveNext(out var contact))
            {
                if (contact.IsTouching && (contact.EntityA == args.OtherEntity || contact.EntityB == args.OtherEntity))
                {
                    // i copypasted this i have no idea what it does
                    Span<Vector2> points = stackalloc Vector2[2];
                    var transformA = _physics.GetPhysicsTransform(contact.EntityA);
                    var transformB = _physics.GetPhysicsTransform(contact.EntityB);
                    contact.GetWorldManifold(transformA, transformB, out var normal, points);
                    int count = 0;
                    foreach (var p in points)
                    {
                        if (p.LengthSquared() > 0.001f) // ignore zero-vectors
                            count++;
                        coord += p;
                    }

                    coord *= 1f / count;
                    break;
                }
            }
            point = coord;
            ourPoint = _transform.ToCoordinates(args.OurEntity, new MapCoordinates(coord, ourXform.MapID));
            otherPoint = _transform.ToCoordinates(args.OtherEntity, new MapCoordinates(coord, otherXform.MapID));

            Log.Debug($"Bugged collision at {args.WorldPoint}, new point: {coord}");

            if (!OnOrNearGrid((uid, ourGrid), ourPoint))
                return;

            if (!OnOrNearGrid((args.OtherEntity, otherGrid), otherPoint))
                return;
        }

        var ourVelocity = _physics.GetLinearVelocity(uid, ourPoint.Position, ourBody, ourXform);
        var otherVelocity = _physics.GetLinearVelocity(args.OtherEntity, otherPoint.Position, otherBody, otherXform);
        var jungleDiff = (ourVelocity - otherVelocity).Length();

        if (jungleDiff < MinimumImpactVelocity || ourXform.MapUid == null)
            return;

        // Play impact sound
        var coordinates = new EntityCoordinates(ourXform.MapUid.Value, point);

        var volume = MathF.Min(10f, 1f * MathF.Pow(jungleDiff, 0.5f) - 5f);
        var audioParams = AudioParams.Default.WithVariation(SharedContentAudioSystem.DefaultVariation).WithVolume(volume);
        _audio.PlayPvs(_shuttleImpactSound, coordinates, audioParams);

        // Convert the collision point directly to tile indices
        var ourTile = new Vector2i((int)Math.Floor(ourPoint.X / ourGrid.TileSize), (int)Math.Floor(ourPoint.Y / ourGrid.TileSize));
        var otherTile = new Vector2i((int)Math.Floor(otherPoint.X / otherGrid.TileSize), (int)Math.Floor(otherPoint.Y / otherGrid.TileSize));

        var ourMass = GetRegionMass(uid, ourGrid, ourTile, ImpactRadius);
        var otherMass = GetRegionMass(args.OtherEntity, otherGrid, otherTile, ImpactRadius);
        Log.Info($"Shuttle impact of {ToPrettyString(uid)} with {ToPrettyString(args.OtherEntity)}; our mass: {ourMass}, other: {otherMass}, velocity {jungleDiff}");

        var energyMult = MathF.Pow(jungleDiff, 2) / 2;
        var biasMult = MathF.Sqrt(ourMass / otherMass); // multiplier to make the area with more mass take less damage so a reinforced wall rammer doesn't die to lattice
        var ourEnergy = ourMass * energyMult * MathF.Min(1f, biasMult);
        var otherEnergy = otherMass * energyMult / MathF.Max(1f, biasMult);

        var ourRadius = Math.Min(ImpactRadius, MathF.Sqrt(otherEnergy / TileBreakEnergyMultiplier / PlatingMass));
        var otherRadius = Math.Min(ImpactRadius, MathF.Sqrt(ourEnergy / TileBreakEnergyMultiplier / PlatingMass));

        var ourPostImpactVelocity = ourVelocity * MathF.Pow(ImpactSlowdown, otherMass / ourBody.FixturesMass);
        var otherPostImpactVelocity = otherVelocity * MathF.Pow(ImpactSlowdown, ourMass / otherBody.FixturesMass);
        _physics.ApplyLinearImpulse(uid, (-ourVelocity + ourPostImpactVelocity) * ourBody.FixturesMass, body: ourBody);
        _physics.ApplyLinearImpulse(args.OtherEntity, (-otherVelocity + otherPostImpactVelocity) * otherBody.FixturesMass, body: otherBody);

        var dir = (ourVelocity.Length() > otherVelocity.Length() ? ourVelocity : -otherVelocity).Normalized();
        ProcessImpactZone(uid, ourGrid, ourTile, otherEnergy, -dir, ourRadius);
        ProcessImpactZone(args.OtherEntity, otherGrid, otherTile, ourEnergy, dir, otherRadius);

        // Compare masses to determine which shuttle should have knockdowns
        if (otherBody.FixturesMass > ourBody.FixturesMass)
            KnockdownEntitiesOnGrid(uid);
        else
            KnockdownEntitiesOnGrid(args.OtherEntity);
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
            if (xform.GridUid != gridUid)
                continue;

            // If entity has a buckle component and is buckled, skip it
            if (buckleQuery.TryGetComponent(uid, out var buckle) && buckle.Buckled)
                continue;

            // Skip if the entity directly has NoSlip component
            if (noSlipQuery.HasComponent(uid))
                continue;

            // Check if they're wearing shoes with NoSlip component or activated magboots
            if (_inventorySystem.TryGetSlotEntity(uid, "shoes", out var shoes) &&
                    (noSlipQuery.HasComponent(shoes) ||
                        (magbootsQuery.HasComponent(shoes) &&
                        itemToggleQuery.TryGetComponent(shoes, out var toggle) &&
                        toggle.Activated
                        )
                    )
                )
                continue;

            // Add entity to knockdown batch
            entitiesToKnockdown.Add(uid);
        }

        // Apply knockdowns sequentially for safety
        foreach (var uid in entitiesToKnockdown)
            _stuns.TryKnockdown(uid, knockdownTime, true);
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

    // this is fairly cold code so i don't think the performance impact of this matters THAT much
    private float GetRegionMass(EntityUid uid, MapGridComponent grid, Vector2i centerTile, float radius)
    {
        var mass = 0f;
        var ceilRadius = (int)MathF.Ceiling(radius);
        HashSet<EntityUid> counted = new();
        for (var x = -ceilRadius; x <= ceilRadius; x++)
        {
            for (var y = -ceilRadius; y <= ceilRadius; y++)
            {
                if (x*x + y*y > radius*radius)
                    continue;

                Vector2i tile = new Vector2i(centerTile.X + x, centerTile.Y + y);
                var tileRef = _mapSys.GetTileRef(uid, grid, tile);
                if (tileRef.Tile != Tile.Empty)
                {
                    var def = (ContentTileDefinition)_tileDef[tileRef.Tile.TypeId];
                    mass += def.Mass;
                }

                foreach (var localUid in _lookup.GetLocalEntitiesIntersecting(uid, tile, gridComp: grid))
                {
                    if (counted.Contains(localUid))
                        continue;

                    if (TryComp<PhysicsComponent>(localUid, out var physics))
                        mass += physics.FixturesMass;

                    counted.Add(localUid);
                }
            }
        }
        return mass;
    }

    /// <summary>
    /// Processes a zone of tiles around the impact point
    /// </summary>
    private void ProcessImpactZone(EntityUid uid, MapGridComponent grid, Vector2i centerTile, float energy, Vector2 dir, float radius)
    {
        // Skip processing if the grid has an anchor component
        if (
            // Goob - not real
            //HasComp<PreventGridAnchorChangesComponent>(uid) ||
            //HasComp<ForceAnchorComponent>(uid) ||
            !HasComp<Robust.Shared.Physics.BroadphaseComponent>(uid))
            return;

        // Create a list of all tiles to process
        var tilesToProcess = new List<ImpactTileData>();

        // Pre-calculate all tiles that need processing
        var ceilRadius = (int)MathF.Ceiling(radius);
        for (var x = -ceilRadius; x <= ceilRadius; x++)
        {
            for (var y = -ceilRadius; y <= ceilRadius; y++)
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

            if (!HasComp<Robust.Shared.Physics.BroadphaseComponent>(uid))
                continue;

            // Process entities on this tile
            var entitiesOnTile = new HashSet<EntityUid>();

            _lookup.GetLocalEntitiesIntersecting(uid, tileData.Tile, entitiesOnTile, gridComp: grid);

            foreach (var localUid in entitiesOnTile)
            {
                // Apply damage scaled by distance but capped to prevent gibbing
                var scaledDamage = tileData.Energy * DamageMultiplier;
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

                    _throwing.TryThrow(localUid, tileData.ThrowDirection * tileData.DistanceFactor);
                }
            }

            // Mark tiles for breaking/effects
            var def = (ContentTileDefinition)_tileDef[_mapSys.GetTileRef(uid, grid, tileData.Tile).Tile.TypeId];
            if (tileData.Energy > def.Mass * TileBreakEnergyMultiplier)
                brokenTiles.Add(tileData.Tile);

            // Mark tiles for spark effects
            if (tileData.Energy > SparkEnergy && tileData.DistanceFactor > 0.7f)
                sparkTiles.Add(tileData.Tile);
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
        // Break tiles
        foreach (var tile in brokenTiles)
            _mapSys.SetTile(new Entity<MapGridComponent>(uid, grid), tile, Tile.Empty);

        // Spawn spark effects
        foreach (var tile in sparkTiles)
        {
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
    }

    // if you want to reuse this, copy into a separate system as a public method
    private bool OnOrNearGrid(
        Entity<MapGridComponent> grid,
        EntityCoordinates at,
        int tolerance = 2
    )
    {
        for (int x = -tolerance; x <= tolerance; x++)
        {
            for (int y = -tolerance; y <= tolerance; y++)
            {
                if (_mapSys.GetTileRef(grid, grid.Comp, at.Offset(new Vector2(x, y))).Tile != Tile.Empty)
                    return true;
            }
        }
        return false;
    }
}
