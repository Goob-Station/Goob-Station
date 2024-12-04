using Content.Shared.Projectiles;
using Content.Shared._White.Standing;
using Content.Shared.Throwing;

namespace Content.Shared._White.Collision.Knockdown;

public sealed class KnockdownOnCollideSystem : EntitySystem
{
    [Dependency] private readonly SharedLayingDownSystem _layingDown = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KnockdownOnCollideComponent, ProjectileHitEvent>(OnProjectileHit);
        SubscribeLocalEvent<KnockdownOnCollideComponent, ThrowDoHitEvent>(OnEntityHit);
    }

    private void OnEntityHit(Entity<KnockdownOnCollideComponent> ent, ref ThrowDoHitEvent args)
    {
        ApplyEffects(args.Target, ent.Comp);
    }

    private void OnProjectileHit(Entity<KnockdownOnCollideComponent> ent, ref ProjectileHitEvent args)
    {
        ApplyEffects(args.Target, ent.Comp);
    }

    private void ApplyEffects(EntityUid target, KnockdownOnCollideComponent component)
    {
        _layingDown.TryLieDown(target, null, null, component.Behavior);
    }
}
