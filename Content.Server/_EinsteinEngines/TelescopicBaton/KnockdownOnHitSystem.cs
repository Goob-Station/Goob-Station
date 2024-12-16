using System.Linq;
using Content.Server.Stunnable;
using Content.Shared._EinsteinEngines.TelescopicBaton;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._EinsteinEngines.TelescopicBaton;

public sealed class KnockdownOnHitSystem : EntitySystem
{
    [Dependency] private readonly StunSystem _stun = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<KnockdownOnHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<KnockdownOnHitComponent> entity, ref MeleeHitEvent args)
    {
        if (!args.IsHit || !args.HitEntities.Any() || entity.Comp.Duration <= TimeSpan.Zero) // Goob edit
            return;

        var ev = new KnockdownOnHitAttemptEvent(false, entity.Comp.DropHeldItemsBehavior); // Goob edit
        RaiseLocalEvent(entity, ref ev);
        if (ev.Cancelled)
            return;

        foreach (var target in args.HitEntities)
        {
            if (!TryComp(target, out StatusEffectsComponent? statusEffects))
                continue;

            _stun.TryKnockdown(target,
                entity.Comp.Duration,
                entity.Comp.RefreshDuration,
                ev.Behavior, // Goob edit
                statusEffects);
        }
    }
}
