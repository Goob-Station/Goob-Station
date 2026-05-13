using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Spawns an entity in each direction specified per YAML.
/// Can support spawn in all cardinal and intercardinal directions.
/// </summary>
public sealed class CardinalSpawnerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CardinalSpawnerComponent, CardinalSpawnerActionEvent>(TrySpawn);
    }

    private void TrySpawn(EntityUid uid, CardinalSpawnerComponent comp, ref CardinalSpawnerActionEvent args)
    {
        var coordinates = Transform(uid).Coordinates;

        // Cardinals
        var north = coordinates.Offset(new Vector2(0, (comp.Offset + 1)));
        var south = coordinates.Offset(new Vector2(0, (-comp.Offset - 1)));
        var west  = coordinates.Offset(new Vector2((-comp.Offset - 1), 0));
        var east  = coordinates.Offset(new Vector2((comp.Offset + 1), 0));

        // Intercardinals
        var northwest = coordinates.Offset(new Vector2((-comp.Offset - 1), (comp.Offset + 1)));
        var northeast = coordinates.Offset(new Vector2((comp.Offset + 1), (comp.Offset + 1)));
        var southwest = coordinates.Offset(new Vector2((-comp.Offset - 1), (-comp.Offset - 1)));
        var southeast = coordinates.Offset(new Vector2((comp.Offset + 1), (-comp.Offset - 1)));

        var directionCoords = new Dictionary<SpawnDirection, EntityCoordinates>
        {
            {SpawnDirection.North, north},
            {SpawnDirection.South, south},
            {SpawnDirection.West, west},
            {SpawnDirection.East, east},

            {SpawnDirection.Northwest, northwest},
            {SpawnDirection.Northeast, northeast},
            {SpawnDirection.Southwest, southwest},
            {SpawnDirection.Southeast, southeast},
        };

        if (comp.AllDirections)
        {
            // If you're using AllDirections you should be using StandardPrototype.
            comp.Directions[SpawnDirection.North] = comp.StandardPrototype!.Value;
            comp.Directions[SpawnDirection.South] = comp.StandardPrototype!.Value;
            comp.Directions[SpawnDirection.East] = comp.StandardPrototype!.Value;
            comp.Directions[SpawnDirection.West] = comp.StandardPrototype!.Value;
            comp.Directions[SpawnDirection.Northeast] = comp.StandardPrototype!.Value;
            comp.Directions[SpawnDirection.Northwest] = comp.StandardPrototype!.Value;
            comp.Directions[SpawnDirection.Southeast] = comp.StandardPrototype!.Value;
            comp.Directions[SpawnDirection.Southwest] = comp.StandardPrototype!.Value;
        }
        foreach (var (direction, proto) in comp.Directions)
        {
            // Defaults to standard prototype if set.
            var protoToSpawn = comp.StandardPrototype ?? proto;
            var spawnCoords = directionCoords[direction];

            if (comp.SpawnAttached)
            {
                PredictedSpawnAttachedTo(protoToSpawn, spawnCoords, null);
            }
            else
            {
                PredictedSpawnAtPosition(protoToSpawn, spawnCoords, null);
            }
        }

    }
}
