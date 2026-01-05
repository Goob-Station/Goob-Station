// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Mobs.Systems;

namespace Content.Shared.Chat;

public sealed class SharedSuicideSystem : EntitySystem
{
    private static readonly ProtoId<DamageTypePrototype> FallbackDamageType = "Blunt";

    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ConsciousnessSystem _consciousness = default!; // Shitmed Change
    [Dependency] private readonly MobThresholdSystem _mobThresholdSystem = default!; // Goobstation

    /// <summary>
    /// Applies lethal damage spread out across the damage types given.
    /// </summary>
    public void ApplyLethalDamage(Entity<DamageableComponent> target, DamageSpecifier damageSpecifier)
    {
        if (TryComp<ConsciousnessComponent>(target, out var victimConsciousness)) // Shitmed Change
            KillConsciousness((target, victimConsciousness));

        // Create a new damageSpecifier so that we don't make alterations to the original DamageSpecifier
        // Failing  to do this will permanently change a weapon's damage making it insta-kill people
        var appliedDamageSpecifier = new DamageSpecifier(damageSpecifier);
        if (!TryComp<MobThresholdsComponent>(target, out var mobThresholds))
            return;

        // Mob thresholds are sorted from alive -> crit -> dead,
        // grabbing the last key will give us how much damage is needed to kill a target from zero
        // The exact lethal damage amount is adjusted based on their current damage taken
        var lethalAmountOfDamage = mobThresholds.Thresholds.Keys.Last() - _mobThresholdSystem.CheckVitalDamage(target, target.Comp); // Goobstation
        var totalDamage = appliedDamageSpecifier.GetTotal();

        // Removing structural because it causes issues against entities that cannot take structural damage,
        // then getting the total to use in calculations for spreading out damage.
        appliedDamageSpecifier.DamageDict.Remove("Structural");

        // Split the total amount of damage needed to kill the target by every damage type in the DamageSpecifier
        foreach (var (key, value) in appliedDamageSpecifier.DamageDict)
        {
            appliedDamageSpecifier.DamageDict[key] = Math.Ceiling((double) (value * lethalAmountOfDamage / totalDamage));
        }

        _damageableSystem.TryChangeDamage(target, appliedDamageSpecifier, true, origin: target, targetPart: TargetBodyPart.Chest); // Shitmed Change
        Dirty(target, target.Comp);
    }

    /// <summary>
    /// Applies lethal damage in a single type, specified by a single damage type.
    /// </summary>
    public void ApplyLethalDamage(Entity<DamageableComponent> target, ProtoId<DamageTypePrototype>? damageType)
    {
        if (TryComp<ConsciousnessComponent>(target, out var victimConsciousness)) // Shitmed Change
            KillConsciousness((target, victimConsciousness));

        if (!TryComp<MobThresholdsComponent>(target, out var mobThresholds))
            return;

        // Mob thresholds are sorted from alive -> crit -> dead,
        // grabbing the last key will give us how much damage is needed to kill a target from zero
        // The exact lethal damage amount is adjusted based on their current damage taken
        var lethalAmountOfDamage = mobThresholds.Thresholds.Keys.Last() - _mobThresholdSystem.CheckVitalDamage(target, target.Comp); // Goobstation

        // We don't want structural damage for the same reasons listed above
        if (!_prototypeManager.TryIndex(damageType, out var damagePrototype) || damagePrototype.ID == "Structural")
        {
            Log.Error($"{nameof(SharedSuicideSystem)} could not find the damage type prototype associated with {damageType}. Falling back to {FallbackDamageType}");
            damagePrototype = _prototypeManager.Index(FallbackDamageType);
        }

        var damage = new DamageSpecifier(damagePrototype, lethalAmountOfDamage);
        _damageableSystem.TryChangeDamage(target, damage, true, origin: target, targetPart: TargetBodyPart.Chest); // Shitmed Change
        Dirty(target, target.Comp);
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
