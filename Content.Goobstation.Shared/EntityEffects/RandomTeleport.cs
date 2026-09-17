using Content.Goobstation.Shared.Teleportation.Systems;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.EntityEffects;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class RandomTeleport : EntityEffectBase<RandomTeleport>
{
    /// <summary>
    /// Up to how far to teleport the user in tiles.
    /// </summary>
    [DataField]
    public MinMax Radius = new MinMax(5, 20);

    /// <summary>
    /// How many times to try to pick the destination. Larger number means the teleport is more likely to be safe.
    /// </summary>
    [DataField]
    public int TeleportAttempts = 10;
}

public sealed partial class RandomTeleportEffectSystem : EntityEffectSystem<TransformComponent, RandomTeleport>
{
    [Dependency] private SharedRandomTeleportSystem _teleport = default!;

    protected override void Effect(Entity<TransformComponent> ent, ref EntityEffectEvent<RandomTeleport> args)
    {
        _teleport.RandomTeleport(ent, args.Effect.Radius, args.Effect.TeleportAttempts, user: args.User);
    }
}
