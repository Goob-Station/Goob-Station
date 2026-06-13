using Content.Shared.EntityEffects;
using Content.Shared.Projectiles;

namespace Content.Goobstation.Server.Mimery;

public sealed class EntityEffectOnProjectileHitSystem : EntitySystem
{
    [Dependency] private readonly SharedEntityEffectsSystem _effect = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntityEffectOnProjectileHitComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<EntityEffectOnProjectileHitComponent> ent, ref ProjectileHitEvent args)
    {
        foreach (var effect in ent.Comp.Effects)
        {
            _effect.TryApplyEffect(args.Target, effect);
        }
    }
}
