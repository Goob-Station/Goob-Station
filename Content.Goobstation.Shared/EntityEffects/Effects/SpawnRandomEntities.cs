// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects.EntitySpawning;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.EntityEffects.Effects;

/// <summary>
/// Spawns a random number of entities at the target.
/// <see cref="BaseSpawnEntityEntityEffect{T}.Number"/> is the inclusive maximum number of entities to spawn, the minimum is <see cref="Min"/>.
/// </summary>
public sealed partial class SpawnRandomEntities : BaseSpawnEntityEntityEffect<SpawnRandomEntities>
{
    [DataField]
    public int Min = 1;
}

public sealed partial class SpawnRandomEntitiesEffectSystem : EntityEffectSystem<TransformComponent, SpawnRandomEntities>
{
    [Dependency] private IRobustRandom _random = default!;

    protected override void Effect(Entity<TransformComponent> ent, ref EntityEffectEvent<SpawnRandomEntities> args)
    {
        var quantity = _random.Next(args.Effect.Min, args.Effect.Number + 1);

        var proto = args.Effect.Entity;
        for (var i = 0; i < quantity; i++)
        {
            PredictedSpawnNextToOrDrop(proto, ent);
        }
    }
}
