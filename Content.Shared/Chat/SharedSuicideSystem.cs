using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using System.Linq;

// Shitmed Change
using Content.Shared._Shitmed.Medical.Surgery.Consciousness;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;
using Content.Shared.Body.Components;

namespace Content.Shared.Chat;

public sealed class SharedSuicideSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ConsciousnessSystem _consciousness = default!; // Shitmed Change

    /// <summary>
    /// Applies lethal damage spread out across the damage types given.
    /// </summary>
    public void ApplyLethalDamage(Entity<DamageableComponent> target, DamageSpecifier damageSpecifier)
    {
        // Shitmed Change Start
        if (TryComp<ConsciousnessComponent>(target, out var victimConsciousness))
        {
            KillConsciousness((target, victimConsciousness));
            return;
        }
        // Shitmed Change End

        // Create a new damageSpecifier so that we don't make alterations to the original DamageSpecifier
        // Failing  to do this will permanently change a weapon's damage making it insta-kill people
        var appliedDamageSpecifier = new DamageSpecifier(damageSpecifier);
        if (!TryComp<MobThresholdsComponent>(target, out var mobThresholds))
            return;

        // Mob thresholds are sorted from alive -> crit -> dead,
        // grabbing the last key will give us how much damage is needed to kill a target from zero
        // The exact lethal damage amount is adjusted based on their current damage taken
        var lethalAmountOfDamage = mobThresholds.Thresholds.Keys.Last() - target.Comp.TotalDamage;
        var totalDamage = appliedDamageSpecifier.GetTotal();

        // Removing structural because it causes issues against entities that cannot take structural damage,
        // then getting the total to use in calculations for spreading out damage.
        appliedDamageSpecifier.DamageDict.Remove("Structural");

        // Split the total amount of damage needed to kill the target by every damage type in the DamageSpecifier
        foreach (var (key, value) in appliedDamageSpecifier.DamageDict)
        {
            appliedDamageSpecifier.DamageDict[key] = Math.Ceiling((double) (value * lethalAmountOfDamage / totalDamage));
        }

        _damageableSystem.TryChangeDamage(target, appliedDamageSpecifier, true, origin: target);
    }

    /// <summary>
    /// Applies lethal damage in a single type, specified by a single damage type.
    /// </summary>
    public void ApplyLethalDamage(Entity<DamageableComponent> target, ProtoId<DamageTypePrototype>? damageType)
    {
        // Shitmed Change Start
        if (TryComp<ConsciousnessComponent>(target, out var victimConsciousness))
        {
            // redirect suicide to consciousness
            KillConsciousness((target, victimConsciousness));
            return;
        }
        // Shitmed Change End

        if (!TryComp<MobThresholdsComponent>(target, out var mobThresholds))
            return;

        // Mob thresholds are sorted from alive -> crit -> dead,
        // grabbing the last key will give us how much damage is needed to kill a target from zero
        // The exact lethal damage amount is adjusted based on their current damage taken
        var lethalAmountOfDamage = mobThresholds.Thresholds.Keys.Last() - target.Comp.TotalDamage;

        // We don't want structural damage for the same reasons listed above
        if (!_prototypeManager.TryIndex(damageType, out var damagePrototype) || damagePrototype.ID == "Structural")
        {
            Log.Error($"{nameof(SharedSuicideSystem)} could not find the damage type prototype associated with {damageType}. Falling back to Blunt");
            damagePrototype = _prototypeManager.Index<DamageTypePrototype>("Blunt");
        }

        var damage = new DamageSpecifier(damagePrototype, lethalAmountOfDamage);
        _damageableSystem.TryChangeDamage(target, damage, true, origin: target);
    }

    /// <summary>
    ///     Kills a consciousness. lol
    /// </summary>
    public void KillConsciousness(Entity<ConsciousnessComponent> target)
    {
        foreach (var modifier in target.Comp.Modifiers)
            _consciousness.RemoveConsciousnessModifier(target, modifier.Key.Item1, modifier.Key.Item2);

        foreach (var multiplier in target.Comp.Multipliers)
            _consciousness.RemoveConsciousnessMultiplier(target, multiplier.Key.Item1, multiplier.Key.Item2, target);

        _consciousness.AddConsciousnessModifier(target, target, -target.Comp.Cap, "Suicide", ConsciousnessModType.Pain, consciousness: target);
        _consciousness.AddConsciousnessMultiplier(target, target, 0f, "Suicide", ConsciousnessModType.Pain, consciousness: target);
    }
}
