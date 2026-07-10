// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Spawners.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Friends.Components; // Shitmed Change
using Content.Shared._Shitmed.Spawners.EntitySystems; // Shitmed Change

namespace Content.Server.Spawners.EntitySystems;

public sealed class SpawnerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly List<(EntityUid Uid, TimedSpawnerComponent Comp)> _toFire = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TimedSpawnerComponent, MapInitEvent>(OnSpawnerInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _toFire.Clear();
        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<TimedSpawnerComponent>();
        while (query.MoveNext(out var uid, out var timedSpawner))
        {
            if (timedSpawner.NextFire > curTime)
                continue;

            while (timedSpawner.NextFire <= curTime)
                timedSpawner.NextFire += TimeSpan.FromSeconds(timedSpawner.IntervalSeconds);

            _toFire.Add((uid, timedSpawner));
        }

        foreach (var (uid, comp) in _toFire)
            OnTimerFired(uid, comp);
    }

    private void OnSpawnerInit(EntityUid uid, TimedSpawnerComponent component, MapInitEvent args)
    {
        component.NextFire = _timing.CurTime + TimeSpan.FromSeconds(component.IntervalSeconds);
    }

    private void OnTimerFired(EntityUid uid, TimedSpawnerComponent component)
    {
        // Pirate/Starlight: spiderlings only mature while alive.
        if ((component.RequiredState != MobState.Invalid &&
             (!TryComp<MobStateComponent>(uid, out var stateComp) || stateComp.CurrentState != component.RequiredState)) ||
            !_random.Prob(component.Chance) ||
            component.Prototypes.Count == 0)
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

        if (component.DespawnWhenDone)
            QueueDel(uid);
    }
}
