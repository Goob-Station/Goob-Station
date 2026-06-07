// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Systems; // Shitmed Change

namespace Content.Shared._Goobstation.Wizard.SanguineStrike;

public abstract class SharedSanguineStrikeSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly PainSystem _pain = default!;
    [Dependency] private readonly ConsciousnessSystem _consciousness = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SanguineStrikeComponent, MeleeHitEvent>(OnHit);
        SubscribeLocalEvent<SanguineStrikeComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<SanguineStrikeComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("sanguine-strike-examine"));
    }

    private void OnHit(Entity<SanguineStrikeComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        if (args.HitEntities.Contains(args.User))
            return;

        var mobStateQuery = GetEntityQuery<MobStateComponent>();
        var hitMobs = args.HitEntities
            .Where(x => mobStateQuery.TryComp(x, out var mobState) && mobState.CurrentState != MobState.Dead)
            .ToList();
        if (hitMobs.Count == 0)
            return;

        var (uid, comp) = ent;

        var damageWithoutStructural = args.BaseDamage;
        damageWithoutStructural.DamageDict.Remove("Structural");
        var damage = damageWithoutStructural * comp.DamageMultiplier;
        var totalBaseDamage = damageWithoutStructural.GetTotal();
        var totalDamage = totalBaseDamage * comp.DamageMultiplier;
        if (totalDamage > 0f && totalDamage > comp.MaxDamageModifier)
        {
            damage *= comp.MaxDamageModifier / totalDamage;
            damage += damageWithoutStructural;
        }

        var newTotalDamage = damage.GetTotal();
        if (newTotalDamage > totalBaseDamage)
            args.BonusDamage += damage - damageWithoutStructural;
        args.HitSoundOverride = comp.HitSound;

        LifeSteal(args.User, newTotalDamage);

        Hit(uid, comp, args.User, hitMobs);
    }

    protected virtual void Hit(EntityUid uid,
        SanguineStrikeComponent component,
        EntityUid user,
        IReadOnlyList<EntityUid> hitEntities)
    {
    }

    public virtual void BloodSteal(EntityUid user,
        IReadOnlyList<EntityUid> hitEntities,
        FixedPoint2 bloodStealAmount,
        EntityCoordinates? bloodSpillCoordinates)
    {
    }

    public virtual void ParticleEffects(EntityUid user, IReadOnlyList<EntityUid> targets, EntProtoId particle)
    {
    }

    public void LifeSteal(EntityUid uid, FixedPoint2 amount, DamageableComponent? damageable = null, ConsciousnessComponent? consciousness = null)
    {
        if (!Resolve(uid, ref damageable, false))
            return;

        if (Resolve(uid, ref consciousness, false))
        {
            if (consciousness.NerveSystem != default)
            {
                foreach (var painModifier in consciousness.NerveSystem.Comp.Modifiers)
                {
                    _pain.TryRemovePainModifier(consciousness.NerveSystem.Owner,
                        painModifier.Key.Item1,
                        painModifier.Key.Item2,
                        consciousness.NerveSystem.Comp);
                }

                foreach (var painMultiplier in consciousness.NerveSystem.Comp.Multipliers)
                {
                    _pain.TryRemovePainMultiplier(consciousness.NerveSystem.Owner,
                        painMultiplier.Key,
                        consciousness.NerveSystem.Comp);
                }


                foreach (var nerve in consciousness.NerveSystem.Comp.Nerves)
                {
                    foreach (var painFeelsModifier in nerve.Value.PainFeelingModifiers)
                    {
                        _pain.TryRemovePainFeelsModifier(painFeelsModifier.Key.Item1,
                            painFeelsModifier.Key.Item2,
                            nerve.Key,
                            nerve.Value);
                    }
                }
            }

            foreach (var multiplier in
                     consciousness.Multipliers.Where(multiplier => multiplier.Value.Type == ConsciousnessModType.Pain))
            {
                _consciousness.RemoveConsciousnessMultiplier(uid, multiplier.Key.Item1, multiplier.Key.Item2, consciousness);
            }

            foreach (var modifier in
                     consciousness.Modifiers.Where(modifier => modifier.Value.Type == ConsciousnessModType.Pain))
            {
                _consciousness.RemoveConsciousnessModifier(uid, modifier.Key.Item1, modifier.Key.Item2, consciousness);
            }
        }

        var totalUserDamage = damageable.TotalDamage;
        if (totalUserDamage <= FixedPoint2.Zero)
            return;

        DamageSpecifier toHeal;
        if (amount < totalUserDamage)
            toHeal = damageable.Damage * amount / totalUserDamage;
        else
            toHeal = damageable.Damage;

        _damageable.TryChangeDamage(uid,
            -toHeal,
            true,
            false,
            damageable,
            null,
            false,
            targetPart: TargetBodyPart.All,
            splitDamage: SplitDamageBehavior.SplitEnsureAll);
    }
}
