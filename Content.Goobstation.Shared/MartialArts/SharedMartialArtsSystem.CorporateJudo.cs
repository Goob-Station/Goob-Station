// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lincoln McQueen <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.MartialArts;

public partial class SharedMartialArtsSystem
{
    private void InitializeCorporateJudo()
    {
        SubscribeLocalEvent<CanPerformComboComponent, JudoDiscombobulatePerformedEvent>(OnJudoDiscombobulate);
        SubscribeLocalEvent<CanPerformComboComponent, JudoEyePokePerformedEvent>(OnJudoEyePoke);
        SubscribeLocalEvent<CanPerformComboComponent, JudoThrowPerformedEvent>(OnJudoThrow);
        SubscribeLocalEvent<CanPerformComboComponent, JudoArmbarPerformedEvent>(OnJudoArmbar);
        SubscribeLocalEvent<CanPerformComboComponent, JudoWheelThrowPerformedEvent>(OnJudoWheelThrow);
        SubscribeLocalEvent<CanPerformComboComponent, JudoGoldenBlastPerformedEvent>(OnJudoGoldenBlast);

        SubscribeLocalEvent<GrantCorporateJudoComponent, ClothingGotEquippedEvent>(OnGrantCorporateJudo);
        SubscribeLocalEvent<GrantCorporateJudoComponent, ClothingGotUnequippedEvent>(OnRemoveCorporateJudo);
    }

    #region Generic Methods

    private void OnGrantCorporateJudo(Entity<GrantCorporateJudoComponent> ent, ref ClothingGotEquippedEvent args)
    {
        if (!_netManager.IsServer)
            return;

        var user = args.Wearer;
        TryGrantMartialArt(user, ent.Comp);
    }

    private void OnRemoveCorporateJudo(Entity<GrantCorporateJudoComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        var user = args.Wearer;
        if (!TryComp<MartialArtsKnowledgeComponent>(user, out var martialArtsKnowledge))
            return;

        if (martialArtsKnowledge.MartialArtsForm != MartialArtsForms.CorporateJudo)
            return;

        if(!TryComp<MeleeWeaponComponent>(args.Wearer, out var meleeWeaponComponent))
            return;

        var originalDamage = new DamageSpecifier();
        originalDamage.DamageDict[martialArtsKnowledge.OriginalFistDamageType] = FixedPoint2.New(martialArtsKnowledge.OriginalFistDamage);
        meleeWeaponComponent.Damage = originalDamage;

        RemComp<MartialArtsKnowledgeComponent>(user);
        RemComp<CanPerformComboComponent>(user);
    }

    #endregion

    #region Combo Methods

    private void OnJudoDiscombobulate(Entity<CanPerformComboComponent> ent, ref JudoDiscombobulatePerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
        || !TryUseMartialArt(ent, proto, out var target, out _)
        || !TryComp(target, out StatusEffectsComponent? status))
            return;

        _stun.TrySlowdown(target, TimeSpan.FromSeconds(5), true, 0.5f, 0.5f, status);

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, applyResistances: true);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnJudoEyePoke(Entity<CanPerformComboComponent> ent, ref JudoEyePokePerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
        || !TryUseMartialArt(ent, proto, out var target, out _)
        || !TryComp(target, out StatusEffectsComponent? status))
            return;

        _status.TryAddStatusEffect<TemporaryBlindnessComponent>(target,
            "TemporaryBlindness",
            TimeSpan.FromSeconds(2),
            true,
            status);

        _status.TryAddStatusEffect<BlurryVisionComponent>(target,
            "BlurryVision",
            TimeSpan.FromSeconds(5),
            true,
            status);

        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnJudoThrow(Entity<CanPerformComboComponent> ent, ref JudoThrowPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
        || !TryUseMartialArt(ent, proto, out var target, out var downed)
        || downed
        || !TryComp<PullableComponent>(target, out var pullable))
            return;

        _stun.TryKnockdown(target, TimeSpan.FromSeconds(proto.ParalyzeTime), true, proto.DropHeldItemsBehavior);

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, applyResistances: true);

        _pulling.TryStopPull(target, pullable, ent, true);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnJudoArmbar(Entity<CanPerformComboComponent> ent, ref JudoArmbarPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
        || !TryUseMartialArt(ent, proto, out var target, out var downed)
        || !downed
        || !TryComp<PullerComponent>(ent, out var puller)
        || !TryComp<PullableComponent>(target, out var pullable))
            return;

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, applyResistances: true);

        // Taking someone in an armbar should be an equivalent of taking them in a choke grab
        _pulling.TrySetGrabStages((ent, puller), (target, pullable), GrabStage.Suffocate);

        if (TryComp<StandingStateComponent>(ent, out var standing) && standing.Standing)
            _stun.TryKnockdown(target, TimeSpan.FromSeconds(proto.ParalyzeTime), true, proto.DropHeldItemsBehavior);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnJudoWheelThrow(Entity<CanPerformComboComponent> ent, ref JudoWheelThrowPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
        || !TryUseMartialArt(ent, proto, out var target, out var downed)
        || !downed
        || !TryComp<PullableComponent>(target, out var pullable)
        || pullable.Puller != ent
        || pullable.GrabStage != GrabStage.Suffocate)
            return;

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, applyResistances: true);

        _pulling.TryStopPull(target, pullable, ent, true);
        _grabThrowing.Throw(target, ent, _transform.GetMapCoordinates(ent).Position - _transform.GetMapCoordinates(target).Position, 7.5f);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    // Not implemented yet, but I'll leave it here
    private void OnJudoGoldenBlast(Entity<CanPerformComboComponent> ent, ref JudoGoldenBlastPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
        || !TryUseMartialArt(ent, proto, out var target, out var _)
        || !TryComp(target, out StatusEffectsComponent? status)
        || !TryComp<PullableComponent>(target, out var pullable))
            return;

        _stun.TryParalyze(target, TimeSpan.FromSeconds(proto.ParalyzeTime), true, status);

        _pulling.TryStopPull(target, pullable, ent, true);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    #endregion
}
