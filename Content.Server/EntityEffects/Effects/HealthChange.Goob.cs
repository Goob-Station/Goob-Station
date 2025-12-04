using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Goobstation.Maths.FixedPoint;

// Shitmed Changes
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Heretic;

namespace Content.Server.EntityEffects.Effects;

public sealed partial class HealthChange
{
    /// <summary>
    ///     Applies damage (airloss/poison/overdose) to an entity's vital body parts (Head, Chest, Groin).
    ///     Healing effects are applied normally according to the effect's targeting settings.
    ///     Handles poison immunity by reversing damage into healing if the target is immune.
    /// </summary>
    /// <param name="damageSpec">The damage specification to apply</param>
    /// <param name="scale">Scaling factor to apply to the damage values</param>
    /// <param name="args">Effect arguments containing the target entity and other context</param>
    /// <remarks>
    ///     This method splits damage into positive (damage) and negative (healing) components.
    ///     Positive damage is always directed at vital body parts, while healing respects the effect's
    ///     original targeting settings. Poison immunity will convert incoming damage into healing.
    /// </remarks>
    private void ApplyEtcDamageToVitals(DamageSpecifier damageSpec, FixedPoint2 scale, EntityEffectBaseArgs args)
    {
        var ev = new ImmuneToPoisonDamageEvent();
        args.EntityManager.EventBus.RaiseLocalEvent(args.TargetEntity, ref ev);
        if (ev.Immune)
        {
            damageSpec = DamageSpecifier.GetNegative(damageSpec);
            if (damageSpec.GetTotal() == FixedPoint2.Zero)
                return;
        }
        // Goobstation change - poisons and overdose damage apply to vital parts, healing still works like before

        var dmgSys = args.EntityManager.System<DamageableSystem>();

        // Split damage into positive (actual damage) and negative (healing) so we can
        // target damage to vital parts while keeping healing behaviour unchanged.
        var scaled = damageSpec * scale;
        var positive = DamageSpecifier.GetPositive(scaled);
        var negative = DamageSpecifier.GetNegative(scaled);

        // Apply positive damage to vital parts
        if (positive.AnyPositive())
        {
            dmgSys.TryChangeDamage(
                args.TargetEntity,
                positive,
                IgnoreResistances,
                interruptsDoAfters: false,
                targetPart: TargetBodyPart.Vital,
                ignoreBlockers: IgnoreBlockers,
                splitDamage: SplitDamage);
        }

        // Apply healing (negative) with original targeting behavior
        if (!negative.Empty)
        {
            dmgSys.TryChangeDamage(
                args.TargetEntity,
                negative,
                IgnoreResistances,
                interruptsDoAfters: false,
                targetPart: UseTargeting ? TargetPart : null,
                ignoreBlockers: IgnoreBlockers,
                splitDamage: SplitDamage);
        }
    }
}
