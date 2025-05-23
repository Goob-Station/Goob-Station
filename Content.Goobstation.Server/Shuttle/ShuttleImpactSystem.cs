// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// ported from: monolith (Content.Server/Shuttles/Systems/ShuttleSystem.Impact.cs)
using Content.Goobstation.Common.CCVar;
using Content.Server.Shuttles.Components;
using Content.Server.Stunnable;
using Content.Server.Destructible;
using Content.Shared.Audio;
using Content.Shared.Buckle.Components;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Content.Shared.Slippery;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Threading;
using System.Numerics;

namespace Content.Goobstation.Server.Shuttle.Impact;

public sealed partial class ShuttleImpactSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageSys = default!;
    [Dependency] private readonly DestructibleSystem _destructible = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly MapSystem _mapSys = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StunSystem _stuns = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;

    private float MinimumImpactInertia;
    private float MinimumImpactVelocity;
    private float TileBreakEnergyMultiplier;
    private float DamageMultiplier;
    private float StructuralDamage;
    private float SparkEnergy;
    private float ImpactRadius;
    private float ImpactSlowdown;
    private float MinThrowVelocity;
    private float MassBias;
    private float InertiaScaling;

    private const float PlatingMass = 800f;
    private const float BaseShuttleMass = 50f; // shuttle mass to consider the neutral point for inertia scaling

    private readonly SoundCollectionSpecifier _shuttleImpactSound = new("ShuttleImpactSound");

    private EntityQuery<DamageableComponent> _dmgQuery;
    private EntityQuery<PhysicsComponent> _physQuery;
    private EntityQuery<ProjectileComponent> _projQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        SubscribeLocalEvent<ShuttleComponent, StartCollideEvent>(OnShuttleCollide);

        Subs.CVar(_cfg, GoobCVars.MinimumImpactInertia, value => MinimumImpactInertia = value, true);
        Subs.CVar(_cfg, GoobCVars.MinimumImpactVelocity, value => MinimumImpactVelocity = value, true);
        Subs.CVar(_cfg, GoobCVars.TileBreakEnergyMultiplier, value => TileBreakEnergyMultiplier = value, true);
        Subs.CVar(_cfg, GoobCVars.ImpactDamageMultiplier, value => DamageMultiplier = value, true);
        Subs.CVar(_cfg, GoobCVars.ImpactStructuralDamage, value => StructuralDamage = value, true);
        Subs.CVar(_cfg, GoobCVars.SparkEnergy, value => SparkEnergy = value, true);
        Subs.CVar(_cfg, GoobCVars.ImpactRadius, value => ImpactRadius = value, true);
        Subs.CVar(_cfg, GoobCVars.ImpactSlowdown, value => ImpactSlowdown = value, true);
        Subs.CVar(_cfg, GoobCVars.ImpactMinThrowVelocity, value => MinThrowVelocity = value, true);
        Subs.CVar(_cfg, GoobCVars.ImpactMassBias, value => MassBias = value, true);
        Subs.CVar(_cfg, GoobCVars.ImpactInertiaScaling, value => InertiaScaling = value, true);

        _physQuery = GetEntityQuery<PhysicsComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
        _dmgQuery = GetEntityQuery<DamageableComponent>();
        _projQuery = GetEntityQuery<ProjectileComponent>();
    }

    /// <summary>
    /// Handles collision between two shuttles, applying impact damage and effects.
    /// </summary>
    private void OnShuttleCollide(EntityUid uid, ShuttleComponent component, ref StartCollideEvent args)
    {
        if (TerminatingOrDeleted(uid) || EntityManager.IsQueuedForDeletion(uid)
            || TerminatingOrDeleted(args.OtherEntity) || EntityManager.IsQueuedForDeletion(args.OtherEntity)
        )
            return;

        if (!TryComp<MapGridComponent>(uid, out var ourGrid) ||
            !TryComp<MapGridComponent>(args.OtherEntity, out var otherGrid)
        )
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

            if (!OnOrNearGrid((uid, ourGrid), ourPoint) || !OnOrNearGrid((args.OtherEntity, otherGrid), otherPoint))
                return;
        }

        var ourVelocity = _physics.GetLinearVelocity(uid, ourPoint.Position, ourBody, ourXform);
        var otherVelocity = _physics.GetLinearVelocity(args.OtherEntity, otherPoint.Position, otherBody, otherXform);
        var jungleDiff = (ourVelocity - otherVelocity).Length();

        // this is cursed but makes it so that collisions of small grid with large grid count the inertia as being approximately the small grid's
        var effectiveInertiaMult = 1f / (1f / ourBody.FixturesMass + 1f / otherBody.FixturesMass);
        var effectiveInertia = jungleDiff * effectiveInertiaMult;

        if (jungleDiff < MinimumImpactVelocity && effectiveInertia < MinimumImpactInertia
            || ourXform.MapUid == null
            || float.IsNaN(jungleDiff))
            return;

        // Play impact sound
        var coordinates = new EntityCoordinates(ourXform.MapUid.Value, point);

        var volume = MathF.Min(10f, 1f * MathF.Pow(jungleDiff, 0.5f) - 5f);
        var audioParams = AudioParams.Default.WithVariation(SharedContentAudioSystem.DefaultVariation).WithVolume(volume);
        _audio.PlayPvs(_shuttleImpactSound, coordinates, audioParams);

        // Convert the collision point directly to tile indices
        var ourTile = new Vector2i((int)Math.Floor(ourPoint.X / ourGrid.TileSize), (int)Math.Floor(ourPoint.Y / ourGrid.TileSize));
        var otherTile = new Vector2i((int)Math.Floor(otherPoint.X / otherGrid.TileSize), (int)Math.Floor(otherPoint.Y / otherGrid.TileSize));

        var ourMass = GetRegionMass(uid, ourGrid, ourTile, ImpactRadius, out var ourTiles);
        var otherMass = GetRegionMass(args.OtherEntity, otherGrid, otherTile, ImpactRadius, out var otherTiles);
        if (ourTiles == 0 || otherTiles == 0) // i have no idea why this happens
            return;
        Log.Info($"Shuttle impact of {ToPrettyString(uid)} with {ToPrettyString(args.OtherEntity)}; our mass: {ourMass}, other: {otherMass}, velocity {jungleDiff}, impact point {point}");

        var energyMult = MathF.Pow(jungleDiff, 2) / 2;
        // multiplier to make the area with more mass take less damage so a reinforced wall rammer doesn't die to lattice
        var biasMult = MathF.Pow(ourMass / otherMass, MassBias);
        // multiplier to make large grids not just bonk against each other
        var inertiaMult = MathF.Pow(effectiveInertiaMult / BaseShuttleMass, InertiaScaling);
        var ourEnergy = ourMass * energyMult * inertiaMult * MathF.Min(1f, biasMult);
        var otherEnergy = otherMass * energyMult * inertiaMult / MathF.Max(1f, biasMult);

        var ourRadius = Math.Min(ImpactRadius, MathF.Sqrt(otherEnergy / TileBreakEnergyMultiplier / PlatingMass));
        var otherRadius = Math.Min(ImpactRadius, MathF.Sqrt(ourEnergy / TileBreakEnergyMultiplier / PlatingMass));

        var totalInertia = ourVelocity * ourMass + otherVelocity * otherMass;
        var unelasticVel = totalInertia / (ourMass + otherMass);
        var ourPostImpactVelocity = Vector2.Lerp(ourVelocity, unelasticVel, MathF.Min(1f, ImpactSlowdown * ourTiles * args.OurFixture.Density / ourBody.FixturesMass));
        var otherPostImpactVelocity = Vector2.Lerp(otherVelocity, unelasticVel, MathF.Min(1f, ImpactSlowdown * otherTiles * args.OtherFixture.Density / otherBody.FixturesMass));
        var ourDeltaV = -ourVelocity + ourPostImpactVelocity;
        var otherDeltaV = -otherVelocity + otherPostImpactVelocity;
        _physics.ApplyLinearImpulse(uid, ourDeltaV * ourBody.FixturesMass, body: ourBody);
        _physics.ApplyLinearImpulse(args.OtherEntity, otherDeltaV * otherBody.FixturesMass, body: otherBody);

        var dir = (ourVelocity.Length() > otherVelocity.Length() ? ourVelocity : -otherVelocity).Normalized();
        ProcessImpactZone(uid, ourGrid, ourTile, otherEnergy, -dir, ourRadius);
        ProcessImpactZone(args.OtherEntity, otherGrid, otherTile, ourEnergy, dir, otherRadius);

        if (ourDeltaV.Length() > MinImpulseVelocity)
            ThrowEntitiesOnGrid(uid, ourXform, -ourDeltaV);
        if (otherDeltaV.Length() > MinImpulseVelocity)
            ThrowEntitiesOnGrid(args.OtherEntity, otherXform, -otherDeltaV);
    }

    private const float MinImpulseVelocity = 0.1f;

    /// <summary>
    /// Knocks and throws all unbuckled entities on the specified grid.
    /// </summary>
    private void ThrowEntitiesOnGrid(EntityUid gridUid, TransformComponent xform, Vector2 direction)
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

        var childEnumerator = xform.ChildEnumerator;
        var minsq = MinThrowVelocity * MinThrowVelocity;
        while (childEnumerator.MoveNext(out var uid))
        {
            // don't throw static bodies
            if (!_physQuery.TryGetComponent(uid, out var physics) || (physics.BodyType & BodyType.Static) != 0)
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

            if (direction.LengthSquared() > minsq)
            {
                _stuns.TryKnockdown(uid, knockdownTime, true);
                _throwing.TryThrow(uid, direction, physics, Transform(uid), _projQuery, direction.Length(), playSound: false);
            }
            else
            {
                _physics.ApplyLinearImpulse(uid, direction * physics.Mass, body: physics);
            }
        }
    }

    /// <summary>
    /// Structure to hold impact tile processing data
    /// </summary>
    private record struct ImpactTileData(Vector2i Tile, float Energy, float DistanceFactor);

    private float GetRegionMass(EntityUid uid, MapGridComponent grid, Vector2i centerTile, float radius, out int tileCount)
    {
        tileCount = 0;
        var mass = 0f;
        var ceilRadius = (int)MathF.Ceiling(radius);
        HashSet<EntityUid> counted = new();
        HashSet<EntityUid> intersecting = new();
        foreach (var tileRef in _mapSys.GetLocalTilesIntersecting(uid, grid, new Circle(centerTile, radius)))
        {
            var def = (ContentTileDefinition)_tileDef[tileRef.Tile.TypeId];
            mass += def.Mass;
            tileCount++;

            intersecting.Clear();
            _lookup.GetLocalEntitiesIntersecting(uid, tileRef.GridIndices, intersecting, gridComp: grid);
            foreach (var localUid in intersecting)
            {
                if (!counted.Add(localUid))
                    continue;

                if (_physQuery.TryComp(localUid, out var physics))
                    mass += physics.FixturesMass;
            }
        }
        return mass;
    }

    /// <summary>
    /// Processes a zone of tiles around the impact point
    /// </summary>
    private void ProcessImpactZone(EntityUid uid, MapGridComponent grid, Vector2i centerTile, float energy, Vector2 dir, float radius)
    {
        // Create a list of all tiles to process
        var tilesToProcess = new List<ImpactTileData>();

        // Pre-calculate all tiles that need processing
        foreach (var tileRef in _mapSys.GetLocalTilesIntersecting(uid, grid, new Circle(centerTile, radius)))
        {
            var distance = centerTile - tileRef.GridIndices;
            // Calculate distance-based energy falloff
            float distanceFactor = 1.0f - distance.Length / (radius + 1);
            float tileEnergy = energy * distanceFactor;

            tilesToProcess.Add(new ImpactTileData(tileRef.GridIndices, tileEnergy, distanceFactor));
        }

        // Process tiles sequentially for safety
        var brokenTiles = new List<(Vector2i, Tile)>();
        var sparkTiles = new List<Vector2i>();

        ProcessTileBatch(uid, grid, tilesToProcess, dir, 0, tilesToProcess.Count, brokenTiles, sparkTiles);

        // Only proceed with visual effects if the entity still exists
        if (Exists(uid))
        {
            ProcessBrokenTilesAndSparks(uid, grid, brokenTiles, sparkTiles);
        }
    }

    private Vector2 ToTileCenterVec = new Vector2(0.5f, 0.5f);

    /// <summary>
    /// Process a batch of tiles from the impact zone
    /// </summary>
    private void ProcessTileBatch(
        EntityUid uid,
        MapGridComponent grid,
        List<ImpactTileData> tilesToProcess,
        Vector2 throwDirection,
        int startIndex,
        int endIndex,
        List<(Vector2i, Tile)> brokenTiles,
        List<Vector2i> sparkTiles)
    {
        // here so we don't have to `new` it every iteration
        var damageSpec = new DamageSpecifier()
        {
            DamageDict = { ["Blunt"] = 0, ["Structural"] = 0 }
        };

        var entitiesOnTile = new HashSet<Entity<TransformComponent>>();
        for (var i = startIndex; i < endIndex; i++)
        {
            var tileData = tilesToProcess[i];

            bool canBreakTile = true;

            // Process entities on this tile
            entitiesOnTile.Clear();
            _lookup.GetLocalEntitiesIntersecting(uid, tileData.Tile, entitiesOnTile, gridComp: grid);

            // this loop is a hotspot so tell if you know how to optimise it
            foreach (var localEnt in entitiesOnTile)
            {
                // the query can ocassionally return entities barely touching this tile so check for that
                var toCenter = ((Vector2)tileData.Tile + ToTileCenterVec - localEnt.Comp.Coordinates.Position);
                if (MathF.Abs(toCenter.X) > 0.5f || MathF.Abs(toCenter.Y) > 0.5f)
                    continue;

                if (_dmgQuery.TryComp(localEnt, out var damageable))
                {
                    // Apply damage scaled by distance but capped to prevent gibbing
                    var scaledDamage = tileData.Energy * DamageMultiplier;
                    damageSpec.DamageDict["Blunt"] = scaledDamage;
                    damageSpec.DamageDict["Structural"] = scaledDamage * StructuralDamage;

                    _damageSys.TryChangeDamage(localEnt, damageSpec, damageable: damageable);
                }
                // might've been destroyed
                if (TerminatingOrDeleted(localEnt) || EntityManager.IsQueuedForDeletion(localEnt))
                    continue;

                if (!_physQuery.TryComp(localEnt, out var physics))
                    continue;

                // no breaking tiles under walls that haven't been destroyed
                if ((physics.BodyType & BodyType.Static) != 0
                    && (physics.CollisionLayer & (int)CollisionGroup.Impassable) != 0)
                {
                    canBreakTile = false;
                }
                else
                {
                    var direction = throwDirection * tileData.DistanceFactor;
                    _throwing.TryThrow(localEnt, direction, physics, localEnt.Comp, _projQuery, direction.Length(), playSound: false);
                }
            }

            // Mark tiles for spark effects
            if (tileData.Energy > SparkEnergy && tileData.DistanceFactor > 0.7f && _random.Prob(0.2f))
                sparkTiles.Add(tileData.Tile);

            if (!canBreakTile)
                continue;

            // Mark tiles for breaking/effects
            var def = (ContentTileDefinition)_tileDef[_mapSys.GetTileRef(uid, grid, tileData.Tile).Tile.TypeId];
            if (tileData.Energy > def.Mass * TileBreakEnergyMultiplier)
                brokenTiles.Add((tileData.Tile, Tile.Empty));

        }
    }

    /// <summary>
    /// Process visual effects and tile breaking after entity processing
    /// </summary>
    private void ProcessBrokenTilesAndSparks(
        EntityUid uid,
        MapGridComponent grid,
        List<(Vector2i, Tile)> brokenTiles,
        List<Vector2i> sparkTiles)
    {
        // Break tiles
        _mapSys.SetTiles(uid, grid, brokenTiles);

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
        float tolerance = 3f
    )
    {
        var bounds = new Box2(at.Position - new Vector2(tolerance, tolerance), at.Position + new Vector2(tolerance, tolerance));
        // this only finds non-empty tiles so return true if we find anything
        foreach (var tileRef in _mapSys.GetLocalTilesIntersecting(grid, grid.Comp, bounds))
            return true;

        return false;
    }
}
