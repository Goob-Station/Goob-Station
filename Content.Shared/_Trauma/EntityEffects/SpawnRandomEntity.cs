using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects.EntitySpawning;
using Content.Shared.Random.Helpers;
using Robust.Shared.Timing;

namespace Content.Shared._Trauma.EntityEffects;

/// <summary>
/// Spawns a random number of entities at the target.
/// <see cref="Number"/> is the inclusive maximum number of entities to spawn, the minimum is <see cref="Min"/>.
/// </summary>
public sealed partial class SpawnRandomEntities : BaseSpawnEntityEntityEffect<SpawnRandomEntities>
{
    [DataField]
    public int Min = 1;
}

public sealed partial class SpawnRandomEntitiesEffectSystem : EntityEffectSystem<TransformComponent, SpawnRandomEntities>
{
    [Dependency] private IGameTiming _timing = default!;

    protected override void Effect(Entity<TransformComponent> ent, ref EntityEffectEvent<SpawnRandomEntities> args)
    {
        var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));
        var quantity = rand.Next(args.Effect.Min, args.Effect.Number + 1);

        var proto = args.Effect.Entity;
        for (var i = 0; i < quantity; i++)
        {
            PredictedSpawnNextToOrDrop(proto, ent);
        }
    }
}
