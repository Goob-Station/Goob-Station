using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Damage;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee.Events;
using System.Linq;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Deals bonus damage if the target has any of the configured status effects.
/// </summary>
public sealed class BonusDamageOnStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BonusDamageOnStatusEffectComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<BonusDamageOnStatusEffectComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        var multiplier = ent.Comp.DamageMultiplier;

        foreach (var target in args.HitEntities)
        {
            // erm, LINQ is cool, actually.
            var hasRequiredEffect = ent.Comp.RequiredStatusEffects.Any(effect =>
                _status.HasStatusEffect(target, effect));

            if (!hasRequiredEffect)
                continue;

            var extraDamage = new DamageSpecifier();

            foreach (var (type, amount) in args.BaseDamage.DamageDict)
                extraDamage.DamageDict[type] = amount * (multiplier - 1f);

            _damageable.TryChangeDamage(target, extraDamage, origin: ent.Owner);
        }
    }
}
