using Content.Goobstation.Shared.Mimery;
using Content.Shared.EntityEffects;
using Content.Shared.Projectiles;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Mimery;

public sealed class EntityEffectOnProjectileHitSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntityEffectOnProjectileHitComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<EntityEffectOnProjectileHitComponent> ent, ref ProjectileHitEvent args)
    {
        var effectArgs = new EntityEffectBaseArgs(args.Target, EntityManager);
        foreach (var effect in ent.Comp.Effects)
        {
            if (effect.ShouldApply(effectArgs, _random))
                effect.Effect(effectArgs);
        }
    }
}
