// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.BluespaceStorm;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.EntityEffects;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Goobstation.Server.BluespaceStorm;

public enum BluespaceStormPortalPulseEffectType : byte
{
    Explosion,
    ProjectileBurst,
    SpawnEntities,
    SpawnEntitiesAround,
    IgniteNearby,
    ImpulseNearby,
    SpillReagent,
    ReplaceTiles,
    AddGas,
}

public sealed partial class BluespaceStormPortalPulseEffectSystem
    : EntityEffectSystem<TransformComponent, BluespaceStormPortalPulseEffect>
{
    [Dependency] private AtmosphereSystem _atmos = default!;
    [Dependency] private ExplosionSystem _explosion = default!;
    [Dependency] private FlammableSystem _flammable = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private PhysicsSystem _physics = default!;
    [Dependency] private PuddleSystem _puddle = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private GunSystem _gun = default!;
    [Dependency] private BluespaceStormSystem _portals = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
    }

    protected override void Effect(Entity<TransformComponent> entity, ref EntityEffectEvent<BluespaceStormPortalPulseEffect> args)
    {
        var effect = args.Effect;
        var origin = _transform.GetMapCoordinates(entity.Owner);

        switch (effect.Type)
        {
            case BluespaceStormPortalPulseEffectType.Explosion:
                _explosion.QueueExplosion(entity.Owner, effect.ExplosionPrototype, 8, 3, 4, 0, 0);
                break;
            case BluespaceStormPortalPulseEffectType.ProjectileBurst:
                for (var i = 0; i < _random.Next(effect.MinCount, effect.MaxCount); i++)
                {
                    var angle = _random.NextAngle();
                    var projectile = effect.ShootProjectiles
                        ? Spawn(effect.Prototypes[0], origin)
                        : Spawn(effect.Prototypes[0], origin, null, angle);
                    if (effect.ShootProjectiles)
                        _gun.ShootProjectile(projectile, angle.ToVec(), Vector2.Zero, entity.Owner, speed: effect.ProjectileSpeed);
                }
                break;
            case BluespaceStormPortalPulseEffectType.SpawnEntities:
                SpawnEntities(origin, effect, around: false);
                break;
            case BluespaceStormPortalPulseEffectType.SpawnEntitiesAround:
                SpawnEntities(origin, effect, around: true);
                break;
            case BluespaceStormPortalPulseEffectType.IgniteNearby:
            {
                var flammables = new HashSet<Entity<FlammableComponent>>();
                _lookup.GetEntitiesInRange(origin, effect.Range, flammables);
                foreach (var flammable in flammables)
                {
                    _flammable.AdjustFireStacks(flammable.Owner, 1, flammable.Comp);
                    _flammable.Ignite(flammable.Owner, entity.Owner, flammable.Comp);
                }
                break;
            }
            case BluespaceStormPortalPulseEffectType.ImpulseNearby:
                _lookup.GetEntitiesInRange(entity.Owner, effect.Range, _nearbyEntities, LookupFlags.Dynamic | LookupFlags.Sundries);
                foreach (var nearby in _nearbyEntities)
                {
                    if (_physicsQuery.TryGetComponent(nearby, out var physics)
                        && (physics.CollisionMask & (int) CollisionGroup.GhostImpassable) != 0)
                        continue;

                    ApplyRandomImpulse(nearby, effect);
                }
                break;
            case BluespaceStormPortalPulseEffectType.SpillReagent:
            {
                var solution = new Solution();
                solution.AddReagent(effect.Reagent, effect.Amount);
                _puddle.TrySpillAt(Transform(entity.Owner).Coordinates, solution, out _);
                break;
            }
            case BluespaceStormPortalPulseEffectType.ReplaceTiles:
                if (_portals.GetSpawningPoints(entity.Owner, effect.TileSeverity) is { } tiles)
                {
                    var tile = (ContentTileDefinition) _tileDefinitions[effect.Tile];
                    foreach (var tileRef in tiles)
                        _tile.ReplaceTile(tileRef, tile);
                }
                break;
            case BluespaceStormPortalPulseEffectType.AddGas:
                _atmos.GetTileMixture(entity.Owner, excite: true)?.AdjustMoles(effect.Gas, effect.Amount);
                break;
        }
    }

    private readonly HashSet<EntityUid> _nearbyEntities = new();

    [Dependency] private ITileDefinitionManager _tileDefinitions = default!;
    [Dependency] private TileSystem _tile = default!;

    private void SpawnEntities(MapCoordinates origin, BluespaceStormPortalPulseEffect effect, bool around)
    {
        for (var i = 0; i < _random.Next(effect.MinCount, effect.MaxCount); i++)
        {
            var spawnAt = origin;
            if (around)
            {
                spawnAt = new MapCoordinates(
                    origin.X + _random.NextFloat(-effect.Radius, effect.Radius),
                    origin.Y + _random.NextFloat(-effect.Radius, effect.Radius),
                    origin.MapId);
            }

            var spawned = Spawn(_random.Pick(effect.Prototypes), spawnAt);
            if (effect.ApplyImpulse)
                ApplyRandomImpulse(spawned, effect);
        }
    }

    private void ApplyRandomImpulse(EntityUid entity, BluespaceStormPortalPulseEffect effect)
    {
        _physics.ApplyLinearImpulse(entity, new Vector2(
            _random.Next(effect.ImpulseMin, effect.ImpulseMax),
            _random.Next(effect.ImpulseMin, effect.ImpulseMax)));
    }
}

public sealed partial class BluespaceStormPortalPulseEffect : EntityEffectBase<BluespaceStormPortalPulseEffect>
{
    [DataField(required: true)]
    public BluespaceStormPortalPulseEffectType Type;

    [DataField]
    public List<EntProtoId> Prototypes = [];

    [DataField]
    public int MinCount = 1;

    [DataField]
    public int MaxCount = 2;

    [DataField]
    public float Radius;

    [DataField]
    public float Range = 6;

    [DataField]
    public bool ShootProjectiles;

    [DataField]
    public float ProjectileSpeed = 5f;

    [DataField]
    public bool ApplyImpulse;

    [DataField]
    public int ImpulseMin = -15;

    [DataField]
    public int ImpulseMax = 30;

    [DataField]
    public string ExplosionPrototype = "ExplosionAirtightGrid";

    [DataField]
    public string Reagent = string.Empty;

    [DataField]
    public int Amount;

    [DataField]
    public float TileSeverity = 5;

    [DataField]
    public ProtoId<ContentTileDefinition> Tile = "FloorFlesh";

    [DataField]
    public Gas Gas = Gas.Ammonia;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
