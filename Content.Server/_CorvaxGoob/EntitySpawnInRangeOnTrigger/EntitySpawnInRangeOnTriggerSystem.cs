using Content.Server.Explosion.EntitySystems;
using Content.Shared.Physics;
using Robust.Shared.Map.Components;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics;
using System.Numerics;
using System.Linq;
using Robust.Shared.Random;

namespace Content.Server._CorvaxGoob.EntitySpawnInRangeOnTrigger;

public sealed class EntitySpawnInRangeOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] protected readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntitySpawnInRangeOnTriggerComponent, TriggerEvent>(HandleTrigger);
    }

    private void HandleTrigger(Entity<EntitySpawnInRangeOnTriggerComponent> entity, ref TriggerEvent args)
    {
        foreach (var entry in entity.Comp.Entries)
        {
            SpawnEntities(entity, entry);
        }
    }

    private void SpawnEntities(Entity<EntitySpawnInRangeOnTriggerComponent> entity, EntitySpawnInRangeSettingsEntry entry)
    {
        var xform = Transform(entity);
        if (!TryComp(xform.GridUid, out MapGridComponent? grid))
            return;

        var tiles = GetSpawningPoints(entity, entry.Settings);

        if (tiles == null)
            return;

        foreach (var tileref in tiles)
        {
            var coords = _map.ToCenterCoordinates(tileref, grid);

            Spawn(_random.Pick(entry.Spawns), coords);

            if (entity.Comp.SpawnEffect is not null)
                Spawn(entity.Comp.SpawnEffect, coords);
        }
    }

    private List<TileRef>? GetSpawningPoints(EntityUid uid, EntitySpawnSettings settings)
    {
        var xform = Transform(uid);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return null;

        var worldPos = _transform.GetWorldPosition(uid);

        var tilerefs = _map.GetTilesIntersecting(
                xform.GridUid.Value,
                grid,
                new Box2(worldPos + new Vector2(-settings.MaxRange), worldPos + new Vector2(settings.MaxRange)))
            .ToList();

        if (tilerefs.Count == 0)
            return null;

        var physQuery = GetEntityQuery<PhysicsComponent>();
        var resultList = new List<TileRef>();
        while (resultList.Count < _random.Next(settings.MinAmount, settings.MaxAmount))
        {
            if (tilerefs.Count == 0)
                break;

            var tileref = _random.Pick(tilerefs);

            var tileWorldPos = _map.GridTileToWorldPos(xform.GridUid.Value, grid, tileref.GridIndices);
            var distance = Vector2.Distance(tileWorldPos, worldPos);

            if (distance > settings.MaxRange || distance < settings.MinRange)
            {
                tilerefs.Remove(tileref);
                continue;
            }

            if (!settings.CanSpawnOnEntities)
            {
                var valid = true;
                foreach (var ent in _map.GetAnchoredEntities(xform.GridUid.Value, grid, tileref.GridIndices))
                {
                    if (!physQuery.TryGetComponent(ent, out var body))
                        continue;

                    if (body.BodyType != BodyType.Static ||
                        !body.Hard ||
                        (body.CollisionLayer & (int)CollisionGroup.Impassable) == 0)
                        continue;

                    valid = false;
                    break;
                }
                if (!valid)
                {
                    tilerefs.Remove(tileref);
                    continue;
                }
            }

            resultList.Add(tileref);
        }
        return resultList;
    }
}


