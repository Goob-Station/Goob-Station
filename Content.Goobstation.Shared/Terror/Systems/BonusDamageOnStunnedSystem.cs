using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// The name is self explanatory, but if the target is stunned, whichever entity has this comp will deal bonus damage on them.
/// </summary>
public sealed class BonusDamageOnStunnedSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BonusDamageOnStunnedComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<BonusDamageOnStunnedComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        var multiplier = ent.Comp.DamageMultiplier;

        foreach (var target in args.HitEntities)
        {
            if (!TryComp<StatusEffectsComponent>(target, out _) ||(!_status.HasStatusEffect(target, "Stun") && !_status.HasStatusEffect(target, "KnockedDown")))
                continue;

            var extraDamage = new DamageSpecifier();

            foreach (var (type, amount) in args.BaseDamage.DamageDict)
                extraDamage.DamageDict[type] = amount * (multiplier - 1f);

            _damageable.TryChangeDamage(target, extraDamage, origin: ent.Owner);
        }
    }
}
