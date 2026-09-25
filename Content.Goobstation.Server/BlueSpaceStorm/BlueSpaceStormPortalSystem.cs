// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Goobstation.Common.BlueSpaceStorm;
using Content.Server.Chat.Systems;
using Content.Server.Pinpointer;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Throwing;
using Content.Server.Atmos.EntitySystems;
using Content.Server.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos;
using Content.Shared.Physics;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.Physics.Components;
using Content.Shared.Chemistry.Components;
using System.Numerics;
using System.Linq;
using Content.Shared.Chemistry.Reagent;
using Robust.Server.GameObjects;


public sealed class BlueSpaceStormSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly StepTriggerSystem _step = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ITileDefinitionManager _tiledef = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    private HashSet<EntityUid> _entities = new();
    private EntityQuery<PhysicsComponent> _physQuery;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlueSpaceStormPortalComponent, PortalPulseEvent>(OnPortalPulse);
        SubscribeLocalEvent<BlueSpaceStormPortalComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<BlueSpaceStormPortalComponent, PortalMobsAllDeathEvent>(OnMobDeath);
        SubscribeLocalEvent<BlueSpaceStormPortalComponent, PortalMobSpawnEvent>(OnMobSpawn);

    }

    private void OnInit(EntityUid uid, BlueSpaceStormPortalComponent component, ref ComponentStartup args)
    {
        List<EntProtoId> mobsToSpawn = [];
        List<string> possibleSpawns;
        switch (component.PortalType)
        {
            case "LavalandBluespacePortal":
                possibleSpawns = ["PortalGoliath", "PortalWatcherBase", "PortalLegion"];
                for (int i = 0; i < 6; i++)
                {
                    mobsToSpawn.Add(_random.Pick(possibleSpawns));
                }
                break;
            case "PlantBluespacePortal":
                possibleSpawns = ["PortalLivingLight", "PortalLuminousObject", "PortalAngryBee", "PortalTomatoKiller"];
                for (int i = 0; i < 8; i++)
                {
                    mobsToSpawn.Add(_random.Pick(possibleSpawns));
                }
                break;
            case "FleshBluespacePortal":
                possibleSpawns = ["PortalRotHulk", "PortalFleshGolemSalvage", "PortalFleshLoverSalvage", "PortalFleshJaredSalvage", "PortalGhoulProphet"];
                for (int i = 0; i < 7; i++)
                {
                    mobsToSpawn.Add(_random.Pick(possibleSpawns));
                }
                break;
            case "SlimeBluespacePortal":
                possibleSpawns = ["PortalBaseAdultSlime", "PortalPlagueRatMedium", "PortalPlagueRatSmall", "PortalRotHulk", "PortalAncientLegsWraith", "PortalGunbot"];
                for (int i = 0; i < 8; i++)
                {
                    mobsToSpawn.Add(_random.Pick(possibleSpawns));
                }
                break;
        }
        component.MobsToSpawn = mobsToSpawn;
        Timer.Spawn((int) component.TimeForSpawn * 1000, () =>
        {
            if (!TryComp<BlueSpaceStormPortalComponent>(uid, out _))
                return;
            RaiseLocalEvent(uid, new PortalMobSpawnEvent());
        });
        Timer.Spawn((int) component.TimeForPulse * 1000, () =>
        {
            if (!TryComp<BlueSpaceStormPortalComponent>(uid, out _))
                return;
            RaiseLocalEvent(uid, new PortalPulseEvent());
        });
    }

    private void OnPortalPulse(EntityUid uid, BlueSpaceStormPortalComponent component, PortalPulseEvent args)
    {
        var origin = _transform.GetMapCoordinates(uid);
        var rollResult = 100;
        rollResult = _random.Next(0, 101);
        switch (component.PortalType)
        {
            case "LavalandBluespacePortal":
                if (rollResult < 11)
                {
                    _explosionSystem.QueueExplosion(uid, "ExplosionAirtightGrid", 8, 3, 4, 0, 0);
                }
                if (rollResult > 10 && rollResult < 41)
                {
                    for (int i = 0; i < _random.Next(2, 5); i++)
                    {
                        Spawn("ProjectileFireball", origin, null, _random.NextAngle());
                    }

                }
                if (rollResult > 40 && rollResult < 61)
                {
                    for (int i = 0; i < _random.Next(2, 7); i++)
                    {
                        switch (_random.Next(0, 4))
                        {
                            case 0:
                                EntityUid spawnedAsh = Spawn("Ash", origin);
                                _physics.ApplyLinearImpulse(spawnedAsh, new Vector2(_random.Next(-15, 30), _random.Next(-15, 30)));
                                break;
                            case 1:
                                EntityUid spawnedSteel = Spawn("SteelOre", origin);
                                _physics.ApplyLinearImpulse(spawnedSteel, new Vector2(_random.Next(-15, 30), _random.Next(-15, 30)));
                                break;
                            case 2:
                                EntityUid spawnedQuartz = Spawn("OreQuartz", origin);
                                _physics.ApplyLinearImpulse(spawnedQuartz, new Vector2(_random.Next(-15, 30), _random.Next(-15, 30)));
                                break;
                            case 3:
                                EntityUid spawnedCoal = Spawn("OreCoal", origin);
                                _physics.ApplyLinearImpulse(spawnedCoal, new Vector2(_random.Next(-15, 30), _random.Next(-15, 30)));
                                break;
                        }
                    }
                }
                if (rollResult > 60 && rollResult < 81)
                {
                    var flammables = new HashSet<Entity<FlammableComponent>>();
                    _lookup.GetEntitiesInRange(origin, 6, flammables);

                    foreach (var flammable in flammables)
                    {
                        var ent = flammable.Owner;
                        _flammable.AdjustFireStacks(ent, 1, flammable);
                        _flammable.Ignite(ent, uid, flammable);
                    }
                }
                if (rollResult > 80)
                {
                    _lookup.GetEntitiesInRange(uid, 6, _entities, LookupFlags.Dynamic | LookupFlags.Sundries);
                    foreach (var entity in _entities)
                    {
                        if (_physQuery.TryGetComponent(entity, out var phys)
                            && (phys.CollisionMask & (int) CollisionGroup.GhostImpassable) != 0)
                            continue;

                        _physics.ApplyLinearImpulse(entity, new Vector2(_random.Next(-15, 30), _random.Next(-15, 30)));
                    }
                }
                break;
            case "PlantBluespacePortal":
                if (rollResult < 40)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Spawn("BulletLaser", origin, null, _random.NextAngle());
                    }
                }
                else if (rollResult < 90)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        MapCoordinates newOrigin = new(origin.X + _random.NextFloat(-20, 20), origin.Y + _random.NextFloat(-20, 20), origin.MapId);
                        Spawn("KudzuFlowerAngry", newOrigin);
                    }
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                    {
                        MapCoordinates newOrigin = new(origin.X + _random.NextFloat(-20, 20), origin.Y + _random.NextFloat(-20, 20), origin.MapId);
                        Spawn("FloraTree", newOrigin);
                    }
                }
                break;
            case "FleshBluespacePortal":
                if (rollResult < 20)
                {
                    var solution = new Solution();
                    solution.AddReagent(new string("Blood"), 300);
                    _puddle.TrySpillAt(Transform(uid).Coordinates, solution, out _);
                }
                if (rollResult >= 20 && rollResult < 40)
                {
                    var spawnedMeats = new List<EntityUid>();
                    for (int i = 0; i < 5; i++)
                    {
                        var meatSelected = _random.Next(0, 6);
                        switch (meatSelected)
                        {
                            case 0:
                                spawnedMeats.Add(Spawn("FoodMeatGoliath", origin));
                                break;
                            case 1:
                                spawnedMeats.Add(Spawn("FoodMeatRat", origin));
                                break;
                            case 2:
                                spawnedMeats.Add(Spawn("FoodMeatDragon", origin));
                                break;
                            case 3:
                                spawnedMeats.Add(Spawn("FoodMeatHuman", origin));
                                break;
                            case 4:
                                spawnedMeats.Add(Spawn("FoodMeatCorgi", origin));
                                break;
                            case 5:
                                spawnedMeats.Add(Spawn("FoodMeatRotten", origin));
                                break;
                        }
                    }
                    foreach (EntityUid ent in spawnedMeats)
                    {
                        _physics.ApplyLinearImpulse(ent, new Vector2(_random.Next(-15, 30), _random.Next(-15, 30)));
                    }
                }
                if (rollResult >= 40 && rollResult < 50)
                {
                    Spawn("FleshKudzu", origin);
                }
                if (rollResult >= 50)
                {
                    var tiles = GetSpawningPoints(uid, 5, 1);
                    if (tiles != null)
                    {
                        foreach (var tileref in tiles)
                        {
                            ProtoId<ContentTileDefinition> floor = "FloorFlesh";
                            var tile = (ContentTileDefinition) _tiledef[floor];
                            _tile.ReplaceTile(tileref, tile);
                        }
                    }
                }
                break;
            case "SlimeBluespacePortal":
                if (rollResult < 20)
                {
                    var solution = new Solution();
                    solution.AddReagent(new string("Slime"), 300);
                    _puddle.TrySpillAt(Transform(uid).Coordinates, solution, out _);
                }
                if (rollResult >= 20 && rollResult < 40)
                {
                    List<EntityUid> spawnedSyringes = [];
                    for (var i = 0; i < 4; i++)
                    {
                        switch (_random.Next(0, 5))
                        {
                            case 0:
                                spawnedSyringes.Add(Spawn("SyringeHeroin", origin));
                                break;
                            case 1:
                                spawnedSyringes.Add(Spawn("SyringePoisonFent", origin));
                                break;
                            case 2:
                                spawnedSyringes.Add(Spawn("SyringeEphedrine", origin));
                                break;
                            case 3:
                                spawnedSyringes.Add(Spawn("SyringeIpecac", origin));
                                break;
                            case 4:
                                spawnedSyringes.Add(Spawn("SyringeTramadol", origin));
                                break;
                        }
                    }
                    foreach (EntityUid ent in spawnedSyringes)
                    {
                        _physics.ApplyLinearImpulse(ent, new Vector2(_random.Next(-15, 30), _random.Next(-15, 30)));
                    }
                }
                if (rollResult >= 40 && rollResult < 60)
                {
                    var spawnedCorpse = Spawn("SalvageHumanCorpse", origin);
                    _physics.ApplyLinearImpulse(spawnedCorpse, new Vector2(_random.Next(-15, 30), _random.Next(-15, 30)));
                }
                if (rollResult >= 60)
                {
                    var tileMix = _atmos.GetTileMixture(uid, excite: true);
                    tileMix?.AdjustMoles(Gas.Ammonia, 400);
                }
                break;
        }
        Timer.Spawn(component.TimeForPulse * 1000, () =>
        {
            if (!TryComp<BlueSpaceStormPortalComponent>(uid, out _))
                return;
            RaiseLocalEvent(uid, new PortalPulseEvent());
        });
    }

    private void OnMobSpawn(EntityUid uid, BlueSpaceStormPortalComponent component, ref PortalMobSpawnEvent args)
    {
        foreach (EntProtoId proto in component.MobsToSpawn)
        {
            var newSpawn = Spawn(proto, _transform.GetMapCoordinates(uid));
            if(!TryComp<BlueSpaceStormPortalMobComponent>(newSpawn, out _))
            {
                continue;
            }
            else
            {
                component.SpawnedMobs.Add(newSpawn);
                Comp<BlueSpaceStormPortalMobComponent>(newSpawn).LinkedPortal = uid;
            }
        }
    }

    private void OnMobDeath(EntityUid uid, BlueSpaceStormPortalComponent component, ref PortalMobsAllDeathEvent args)
    {
        var query = EntityQueryEnumerator<BlueSpaceRuleComponent, ActiveGameRuleComponent, GameRuleComponent>();
        EntityUid ruleUID;
        while (query.MoveNext(out ruleUID, out var rule, out _, out _))
        {
            rule.PortalsRemaining = Math.Max(0, rule.PortalsRemaining - 1);
            if(rule.PortalsRemaining < 1)
            {
                _chat.DispatchGlobalAnnouncement(
                "The spacetime phenomena has subsided, please return to your stations.",
                colorOverride: Color.Green,
                playSound: false);
                _gameTicker.EndGameRule(ruleUID);
            }
            break;
        }
        _entityManager.DeleteEntity(uid);
    }

    public List<TileRef>? GetSpawningPoints(EntityUid uid, float severity, float powerModifier = 1f)
    {
        var xform = Transform(uid);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return null;


        var amount = (int) (MathHelper.Lerp(2, 5, severity * powerModifier) + 0.5f);

        var worldPos = _transform.GetWorldPosition(uid);

        var tilerefs = _map.GetTilesIntersecting(
                xform.GridUid.Value,
                grid,
                new Box2(worldPos + new Vector2(-10), worldPos + new Vector2(10))).ToList();

        if (tilerefs.Count == 0)
            return null;

        var physQuery = GetEntityQuery<PhysicsComponent>();
        var resultList = new List<TileRef>();
        while (resultList.Count < amount)
        {
            if (tilerefs.Count == 0)
                break;

            var tileref = _random.Pick(tilerefs);

            var tileWorldPos = _map.GridTileToWorldPos(xform.GridUid.Value, grid, tileref.GridIndices);
            var distance = Vector2.Distance(tileWorldPos, worldPos);

            if (distance > 10 || distance < 1)
            {
                tilerefs.Remove(tileref);
                continue;
            }


            resultList.Add(tileref);
        }
        return resultList;
    }

}