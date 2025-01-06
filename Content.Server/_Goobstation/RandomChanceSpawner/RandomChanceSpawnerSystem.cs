using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.RandomChanceSpawner;

public sealed partial class RandomChanceSpawnerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RandomChanceSpawnerComponent, MapInitEvent>(OnMapInit);
    }

    public void OnMapInit(Entity<RandomChanceSpawnerComponent> ent, ref MapInitEvent args)
    {
        foreach(KeyValuePair<EntProtoId, float> kvp in ent.Comp.ToSpawn)
        {
            if (kvp.Value >= _random.NextFloat(0.0f, 1.0f))
                Spawn(kvp.Key, Transform(ent).Coordinates);
        }
        EntityManager.QueueDeleteEntity(ent);
    }
}
