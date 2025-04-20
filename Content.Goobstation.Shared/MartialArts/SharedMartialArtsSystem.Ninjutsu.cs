// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts;

public abstract partial class SharedMartialArtsSystem
{
    public static ProtoId<AlertCategoryPrototype> NinjutsuAlertCategory = "Ninjutsu";

    private void InitializeNinjutsu()
    {
        SubscribeLocalEvent<CanPerformComboComponent, NinjutsuTakedownPerformedEvent>(OnNinjutsuTakedown);
        SubscribeLocalEvent<CanPerformComboComponent, BiteTheDustPerformedEvent>(OnBiteTheDust);
        SubscribeLocalEvent<CanPerformComboComponent, DirtyKillPerformedEvent>(OnDirtyKill);

        SubscribeLocalEvent<GrantNinjutsuComponent, UseInHandEvent>(OnGrantCQCUse);
        SubscribeLocalEvent<GrantNinjutsuComponent, ExaminedEvent>(OnGrantCQCExamine);

        SubscribeLocalEvent<ThrownEvent>(OnThrow);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);

        SubscribeLocalEvent<NinjutsuSneakAttackComponent, SelfBeforeGunShotEvent>(OnBeforeGunShot);
        SubscribeLocalEvent<NinjutsuSneakAttackComponent, AfterComboCheckEvent>(OnNinjutsuAttackPerformed);

        SubscribeLocalEvent<NinjutsuSneakAttackComponent, ComponentInit>(OnSneakAttackInit);
        SubscribeLocalEvent<NinjutsuSneakAttackComponent, ComponentRemove>(OnSneakAttackRemove);
        SubscribeLocalEvent<NinjutsuSneakAttackComponent, StatusEffectEndedEvent>(OnAlertEffectEnded);
    }

    private void OnBeforeGunShot(Entity<NinjutsuSneakAttackComponent> ent, ref SelfBeforeGunShotEvent args)
    {
        ResetDebuff(ent);
    }

    private void OnAlertEffectEnded(Entity<NinjutsuSneakAttackComponent> ent, ref StatusEffectEndedEvent args)
    {
        if (args.Key == "LossOfSurprise")
            _alerts.ShowAlert(ent, ent.Comp.Alert);
    }

    private void OnSneakAttackRemove(Entity<NinjutsuSneakAttackComponent> ent, ref ComponentRemove args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        _alerts.ClearAlertCategory(ent, NinjutsuAlertCategory);
    }

    private void OnSneakAttackInit(Entity<NinjutsuSneakAttackComponent> ent, ref ComponentInit args)
    {
        _alerts.ShowAlert(ent, ent.Comp.Alert);
    }

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        if (!TryComp(ev.Origin, out MartialArtsKnowledgeComponent? knowledge) ||
            knowledge.MartialArtsForm != MartialArtsForms.Ninjutsu)
            return;

        if (ev.NewMobState <= ev.OldMobState)
            return;

        ApplyMultiplier(ev.Origin.Value, 1.2f, 0f, TimeSpan.FromSeconds(3), MartialArtModifierType.MoveSpeed);
        _modifier.RefreshMovementSpeedModifiers(ev.Origin.Value);
    }

    private void OnThrow(ref ThrownEvent ev)
    {
        if (HasComp<NinjutsuSneakAttackComponent>(ev.User))
            ResetDebuff(ev.User.Value);
    }

    private void OnNinjutsuMeleeHit(EntityUid uid, ref MeleeHitEvent ev)
    {
        if (!ev.IsHit)
            return;

        if (!TryComp(uid, out NinjutsuSneakAttackComponent? sneakAttack))
            return;

        if (HasComp<NinjutsuLossOfSurpriseComponent>(uid))
        {
            ResetDebuff(uid);
            return;
        }

        ResetDebuff(uid);

        if (ev.HitEntities.Count == 0)
            return;

        var target = ev.HitEntities[0];
        if (target == uid)
            return;

        if (!IsWeaponValid(uid, ev.Weapon, out _))
            return;

        if (sneakAttack.Multiplier > 1f)
            ev.BonusDamage = ev.BaseDamage * (sneakAttack.Multiplier - 1f);

        // Heavy attack
        if (ev.Direction != null)
            return;

        var total = ev.BaseDamage.GetTotal();
        if (total <= 0f)
            return;

        // Assassinate
        var isUnarmed = uid == ev.Weapon;
        var damageType = isUnarmed ? "Blunt" : "Slash";
        var modifier = isUnarmed ? sneakAttack.AssassinateUnarmedModifier : sneakAttack.AssassinateModifier;
        var bonusDamage = new DamageSpecifier
        {
            DamageDict =
            {
                { damageType, modifier },
            },
        };
        _damageable.TryChangeDamage(target,
            bonusDamage,
            origin: uid,
            armorPenetration: sneakAttack.AssassinateArmorPierce);

        if (_netManager.IsClient)
            return;

        _audio.PlayPvs(isUnarmed ? sneakAttack.AssassinateSoundUnarmed : sneakAttack.AssassinateSoundArmed,
            target);
        ComboPopup(uid, target, sneakAttack.AssassinateComboName);
    }

    private void OnNinjutsuAttackPerformed(Entity<NinjutsuSneakAttackComponent> ent, ref AfterComboCheckEvent args)
    {
        if (!HasComp<NinjutsuLossOfSurpriseComponent>(ent))
        {
            ResetDebuff(ent);
            return;
        }

        ResetDebuff(ent);

        if (args.Type != ComboAttackType.Harm || args.Target == args.Performer ||
            !IsWeaponValid(args.Performer, args.Weapon, out var melee) ||
            !_standingState.IsDown(args.Target))
            return;

        // Swift Strike
        if (args.Performer == args.Weapon)
            _stamina.TakeStaminaDamage(args.Target, 30f, applyResistances: true);
        var fireRate = TimeSpan.FromSeconds(1f / _melee.GetAttackRate(args.Weapon, args.Performer, melee));
        melee.NextAttack -= fireRate / 2f;
        Dirty(args.Weapon, melee);
    }

    private void OnDirtyKill(Entity<CanPerformComboComponent> ent, ref DirtyKillPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed))
            return;

        if (!downed)
        {
            _popupSystem.PopupEntity(Loc.GetString("martial-arts-fail-target-standing"), ent, ent);
            return;
        }

        // Paralyze, not knockdown
        _stun.TryParalyze(target, TimeSpan.FromSeconds(proto.ParalyzeTime), true);
        DoDamage(ent,
            target,
            proto.DamageType,
            proto.ExtraDamage * GetDamageMultiplier(ent),
            out _,
            TargetBodyPart.Torso);
        _audio.PlayPvs(args.Sound, target);
        ComboPopup(ent, target, proto.Name);
    }

    private void OnBiteTheDust(Entity<CanPerformComboComponent> ent, ref BiteTheDustPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed))
            return;

        if (downed)
        {
            _popupSystem.PopupEntity(Loc.GetString("martial-arts-fail-target-down"), ent, ent);
            return;
        }

        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);


        _stun.TryKnockdown(target, TimeSpan.FromSeconds(proto.ParalyzeTime), true, proto.DropHeldItemsBehavior);
        DoDamage(ent,
            target,
            proto.DamageType,
            proto.ExtraDamage * GetDamageMultiplier(ent),
            out _,
            TargetBodyPart.Torso);
        _audio.PlayPvs(args.Sound, target);
        ComboPopup(ent, target, proto.Name);
    }

    private void OnNinjutsuTakedown(Entity<CanPerformComboComponent> ent, ref NinjutsuTakedownPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed))
            return;

        if (!TryComp(ent, out NinjutsuSneakAttackComponent? sneakAttack))
            return;

        if (HasComp<NinjutsuLossOfSurpriseComponent>(ent))
        {
            _popupSystem.PopupEntity(Loc.GetString("ninjutsu-fail-loss-of-surprise"), ent, ent);
            return;
        }

        if (downed)
        {
            _popupSystem.PopupEntity(Loc.GetString("martial-arts-fail-target-down"), ent, ent);
            return;
        }

        float time = proto.ParalyzeTime;
        if (_backstab.TryBackstab(target, ent, Angle.FromDegrees(45d), true, false, false))
            time *= args.BackstabMultiplier;

        _stun.TryKnockdown(target, TimeSpan.FromSeconds(time), true, proto.DropHeldItemsBehavior);
        DoDamage(ent,
            target,
            proto.DamageType,
            proto.ExtraDamage * sneakAttack.Multiplier,
            out _,
            TargetBodyPart.Torso);
        _audio.PlayPvs(args.Sound, target);
        ComboPopup(ent, target, proto.Name);
    }

    private float GetDamageMultiplier(EntityUid uid)
    {
        if (HasComp<NinjutsuLossOfSurpriseComponent>(uid) ||
            !TryComp(uid, out NinjutsuSneakAttackComponent? sneakAttack))
            return 1f;

        return sneakAttack.Multiplier;
    }

    private bool IsWeaponValid(EntityUid user, EntityUid weapon, [NotNullWhen(true)] out MeleeWeaponComponent? melee)
    {
        if (!TryComp(weapon, out melee))
            return false;

        return user == weapon || melee.Damage.DamageDict.ContainsKey("Slash");
    }

    private void ResetDebuff(EntityUid uid)
    {
        _status.TryAddStatusEffect<NinjutsuLossOfSurpriseComponent>(uid,
            "LossOfSurprise",
            TimeSpan.FromSeconds(5),
            true);

        _stealth.TryRevealNinja(uid);
    }
}
