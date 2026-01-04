using Content.Shared._Shitcode.Heretic.Components.StatusEffects;
using Content.Shared.Heretic;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeLock()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticCloak>(OnCloak);

        SubscribeLocalEvent<HereticComponent, HereticAscensionLockEvent>(OnAscensionLock);
    }

    private void OnCloak(Entity<HereticComponent> ent, ref EventHereticCloak args)
    {
        if (_statusNew.TryEffectsWithComp<HereticCloakedStatusEffectComponent>(ent, out var effects))
        {
            foreach (var effect in effects)
            {
                PredictedQueueDel(effect.Owner);
            }

            args.Handled = true;
            return;
        }

        // TryUseAbility only if we are not cloaked so that we can uncloak without focus
        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;
        _statusNew.TryAddStatusEffect(ent, args.Status, out _, args.Lifetime);
    }

    private void OnAscensionLock(Entity<HereticComponent> ent, ref HereticAscensionLockEvent args)
    {
    }
}
