using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Rejuvenate;
using Content.Shared.Tag;
using Content.Shared.Whitelist;

namespace Content.Shared._Goobstation.Wizard.Projectiles;

public sealed class RejuvenateOnProjectileHitSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly TagSystem _tag = default!;

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
            ApplyEffects(comp, args.Target, comp.ReverseEffects);
            return;
        }

        ApplyEffects(comp, args.Target, !comp.ReverseEffects);
    }

    private void ApplyEffects(RejuvenateOnProjectileHitComponent comp, EntityUid target, bool rejuvenate)
    {
        if (rejuvenate)
        {
            if (!_tag.HasTag(target, comp.SoulTappedTag))
                RaiseLocalEvent(target, new RejuvenateEvent(false, false));
            return;
        }

        if (!_mobState.IsDead(target))
        {
            _damageable.TryChangeDamage(target,
                comp.Damage,
                true,
                canSever: false,
                targetPart: TargetBodyPart.Torso);
        }
    }
}
