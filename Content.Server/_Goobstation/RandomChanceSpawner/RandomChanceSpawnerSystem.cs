using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.RandomChanceSpawner;

public sealed partial class RandomChanceSpawnerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        foreach (var comp in EntityManager.EntityQuery<RandomChanceSpawnerComponent>())
        {
            var uid = comp.Owner;

            foreach(KeyValuePair<EntProtoId, float> kvp in comp.ToSpawn)
            {
                if (kvp.Value >= _random.NextFloat(0.0f, 1.0f))
                    Spawn(kvp.Key, Transform(uid).Coordinates);
            }
            EntityManager.QueueDeleteEntity(uid);
        }
    }
}