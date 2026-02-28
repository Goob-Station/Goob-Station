using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage;

namespace Content.Goobstation.Shared.Terror.Systems;

public sealed class BonusDamageOnStunnedSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BonusDamageOnStunnedComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<BonusDamageOnStunnedComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        var multiplier = ent.Comp.DamageMultiplier;
        var anyStunned = false;

        foreach (var target in args.HitEntities)
        {
            if (!TryComp<StatusEffectsComponent>(target, out var status))
                continue;

            var isStunned =
                _status.HasStatusEffect(target, "Stun") ||
                _status.HasStatusEffect(target, "KnockedDown");

            if (isStunned)
            {
                anyStunned = true;
                break;
            }
        }

        if (!anyStunned)
            return;

        // Apply a multiplicative modifier to all damage types
        var modifier = new DamageModifierSet
        {
            Coefficients = new Dictionary<string, float>()
        };

        foreach (var type in args.BaseDamage.DamageDict.Keys)
        {
            modifier.Coefficients[type] = multiplier;
        }

        args.ModifiersList.Add(modifier);
    }
}
