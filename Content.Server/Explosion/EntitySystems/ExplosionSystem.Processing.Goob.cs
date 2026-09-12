using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Destructible;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Destructible.Thresholds.Triggers;

namespace Content.Server.Explosion.EntitySystems;

public sealed partial class ExplosionSystem
{
    // Shitmed Change: This is basically a private implementation handling a "prediction" of
    // whether or not the explosion would trigger damage thresholds on a Woundmed entity.
    // TODO: If it works well over time, move to an event.
    private bool ShitmedWouldTriggerDestructibleThreshold(EntityUid uid, DamageSpecifier incomingDamage, EntityUid? cause)
    {
        if (!TryComp<DestructibleComponent>(uid, out var destructible)
            || !TryComp<DamageableComponent>(uid, out var damageable)
            || !TryComp<BodyComponent>(uid, out var body)
            || body.BodyType == Shared._Shitmed.Body.BodyType.Simple)
            return false;

        foreach (var threshold in destructible.Thresholds)
        {
            // Skip if already triggered and triggers only once
            if (threshold.Triggered && threshold.TriggersOnce)
                continue;

            // Check if this threshold uses a damage type trigger
            if (threshold.Trigger is not DamageTypeTrigger damageTypeTrigger)
                continue;

            // Get current damage for this damage type
            var currentDamage = damageable.Damage.DamageDict.TryGetValue(damageTypeTrigger.DamageType, out var current)
                ? current
                : FixedPoint2.Zero;

            // Get incoming damage for this damage type
            var additionalDamage = incomingDamage.DamageDict.TryGetValue(damageTypeTrigger.DamageType, out var incoming)
                ? incoming
                : FixedPoint2.Zero;

            // Check if combined damage would exceed threshold
            if (currentDamage + additionalDamage >= damageTypeTrigger.Damage)
            {
                _destructibleSystem.Execute(threshold, uid, cause);
                return true;
            }
        }

        return false;
    }

    private void ShitmedHandleGetDamagePartDamageVariationAndWoundSeverityMultipliers(ref DamageSpecifier damage)
    {
        damage *= _damageableSystem.UniversalExplosionDamageModifier;
        if (damage.PartDamageVariation == 0f)
            damage.PartDamageVariation = PartVariation;
        foreach (var type in new List<string> {"Blunt", "Slash", "Piercing", "Heat", "Cold"})
        {
            damage.WoundSeverityMultipliers.TryAdd(type, WoundMultiplier);
        }
    }
}