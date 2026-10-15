// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Spawners.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Friends.Components; // Shitmed Change
using Content.Shared._Shitmed.Spawners.EntitySystems; // Shitmed Change

namespace Content.Server.Spawners.EntitySystems;

public sealed class SpawnerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TimedSpawnerComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<TimedSpawnerComponent>();
        while (query.MoveNext(out var uid, out var timedSpawner))
        {
            if (timedSpawner.NextFire > curTime)
                continue;

            OnTimerFired(uid, timedSpawner);

            timedSpawner.NextFire += TimeSpan.FromSeconds(timedSpawner.IntervalSeconds);
        }
    }

    private void OnMapInit(Entity<TimedSpawnerComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextFire = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.IntervalSeconds);
    }

    private void OnTimerFired(EntityUid uid, TimedSpawnerComponent component)
    {
        if (!_random.Prob(component.Chance))
            return;

        var number = _random.Next(component.MinimumEntitiesSpawned, component.MaximumEntitiesSpawned);
        var coordinates = Transform(uid).Coordinates;

        for (var i = 0; i < number; i++)
        {
            var entity = _random.Pick(component.Prototypes);
            // Shitmed Change Start
            var spawnedEnt = SpawnAtPosition(entity, coordinates);
            var ev = new SpawnerSpawnedEvent(spawnedEnt, HasComp<PettableFriendComponent>(spawnedEnt));
            RaiseLocalEvent(uid, ev);
            // Shitmed Change End
        }
    }
}