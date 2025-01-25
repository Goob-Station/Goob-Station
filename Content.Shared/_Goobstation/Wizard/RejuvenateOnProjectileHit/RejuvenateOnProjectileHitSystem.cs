using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Rejuvenate;
using Content.Shared.Whitelist;

namespace Content.Shared._Goobstation.Wizard.RejuvenateOnProjectileHit;

public sealed class RejuvenateOnProjectileHitSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RejuvenateOnProjectileHitComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<RejuvenateOnProjectileHitComponent> ent, ref ProjectileHitEvent args)
    {
        var (_, comp) = ent;

        if (_whitelist.IsValid(comp.UndeadList, args.Target))
        {
            if (!_mobState.IsDead(args.Target))
            {
                _damageable.TryChangeDamage(args.Target,
                    comp.UndeadDamage,
                    true,
                    canSever: false,
                    targetPart: TargetBodyPart.Torso);
            }
            return;
        }

        RaiseLocalEvent(args.Target, new RejuvenateEvent());
    }
}
