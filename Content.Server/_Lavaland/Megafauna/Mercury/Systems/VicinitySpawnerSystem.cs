using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using SixLabors.ImageSharp;

namespace Content.Server._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// This system handles spawning from a list of entities within a range of the component-haver.
/// It's also just VoidPortal stripped down to be more generic so it's just copy pasted code.
/// </summary>
public sealed class VicinitySpawnerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VicinitySpawnerComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<VicinitySpawnerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.Accumulator)
                continue;

            comp.Accumulator = _timing.CurTime + comp.SpawnInterval;

            TrySpawn(uid, comp);
        }
    }

    private void OnMapInit(Entity<VicinitySpawnerComponent> ent, ref MapInitEvent args)
    {
        TrySpawn(ent.Owner, ent);

        ent.Comp.Accumulator = _timing.CurTime + ent.Comp.SpawnInterval;
    }

    private void TrySpawn(EntityUid uid, VicinitySpawnerComponent comp)
    {
        var transform = Transform(uid);
        var grid = transform.GridUid;
        var center = transform.Coordinates;

        // Checks how many entities it should spawn and spawns them at the same time in different coordinates.
        for (var i = 0; i < comp.NumberToSpawn; i++)
        {
            // Determine valid spawn coordinates
            EntityCoordinates spawnCoords = default;
            var attempts = 0;
            const int MaxAttempts = 250; // Same comment as original portal comment, lower this if this causes any issues.

            do
            {
                attempts++;

                spawnCoords = grid != null
                    ? new EntityCoordinates(
                        grid.Value,
                        center.X + _random.Next(-comp.OffsetForSpawn, comp.OffsetForSpawn + 1),
                        center.Y + _random.Next(-comp.OffsetForSpawn, comp.OffsetForSpawn + 1)
                      )
                    : center;

                // This spawns in an empty prototype, then checks if it is inside an object. Regardless of it is or not, it gets deleted as soon as the confirmation is sent.
                var tempEnt = Spawn(comp.EmptyPrototype, spawnCoords);
                bool obstructed = _physics.GetEntitiesIntersectingBody(tempEnt, (int) CollisionGroup.Impassable).Count > 0;
                QueueDel(tempEnt);

                if (!obstructed)
                    break;

            } while (attempts < MaxAttempts); // If it does not find a unobstructed tile it just spawns anyway at the last coordinate.

            // Decide what to spawn
            EntProtoId protoToSpawn;

            protoToSpawn = _random.Pick(comp.Prototype);

            Spawn(protoToSpawn, spawnCoords);
        }
    }
}
