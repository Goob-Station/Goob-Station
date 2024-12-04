using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Projectiles;
using Content.Shared.StatusEffect;
using Content.Shared.Throwing;

namespace Content.Shared._White.Collision.Blur;

public sealed class BlurOnCollideSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlurOnCollideComponent, ProjectileHitEvent>(OnProjectileHit);
        SubscribeLocalEvent<BlurOnCollideComponent, ThrowDoHitEvent>(OnEntityHit);
    }

    private void OnEntityHit(Entity<BlurOnCollideComponent> ent, ref ThrowDoHitEvent args)
    {
        ApplyEffects(args.Target, ent.Comp);
    }

    private void OnProjectileHit(Entity<BlurOnCollideComponent> ent, ref ProjectileHitEvent args)
    {
        ApplyEffects(args.Target, ent.Comp);
    }

    private void ApplyEffects(EntityUid target, BlurOnCollideComponent component)
    {
        _statusEffects.TryAddStatusEffect<BlurryVisionComponent>(target,
            "BlurryVision",
            TimeSpan.FromSeconds(component.BlurTime),
            true);
    }
}
