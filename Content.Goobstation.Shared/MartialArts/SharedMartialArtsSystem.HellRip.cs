// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lincoln McQueen <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Eye.Blinding.Components;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio;

// Shitmed Change
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;

namespace Content.Goobstation.Shared.MartialArts;

public partial class SharedMartialArtsSystem
{
    private void InitializeHellRip()
    {
        SubscribeLocalEvent<CanPerformComboComponent, HellRipSlamPerformedEvent>(OnHellRipSlam);
        SubscribeLocalEvent<CanPerformComboComponent, HellRipDropKickPerformedEvent>(OnHellRipDropKick);
        SubscribeLocalEvent<CanPerformComboComponent, HellRipHeadRipPerformedEvent>(OnHellRipHeadRip);
        SubscribeLocalEvent<CanPerformComboComponent, HellRipTearDownPerformedEvent>(OnHellRipTearDown);

        SubscribeLocalEvent<GrantHellRipComponent, ClothingGotEquippedEvent>(OnGrantHellRip);
        SubscribeLocalEvent<GrantHellRipComponent, ClothingGotUnequippedEvent>(OnRemoveHellRip);
    }

    #region Generic Methods

    private void OnGrantHellRip(Entity<GrantHellRipComponent> ent, ref ClothingGotEquippedEvent args)
    {
        if (!_netManager.IsServer)
            return;

        var user = args.Wearer;
        TryGrantMartialArt(user, ent.Comp);
    }

    private void OnRemoveHellRip(Entity<GrantHellRipComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        var user = args.Wearer;
        if (!TryComp<MartialArtsKnowledgeComponent>(user, out var martialArtsKnowledge))
            return;

        if (martialArtsKnowledge.MartialArtsForm != MartialArtsForms.HellRip)
            return;

        if (!TryComp<MeleeWeaponComponent>(args.Wearer, out var meleeWeaponComponent))
            return;

        var originalDamage = new DamageSpecifier();
        originalDamage.DamageDict[martialArtsKnowledge.OriginalFistDamageType]
            = FixedPoint2.New(martialArtsKnowledge.OriginalFistDamage);
        meleeWeaponComponent.Damage = originalDamage;

        RemComp<MartialArtsKnowledgeComponent>(user);
        RemComp<CanPerformComboComponent>(user);
    }

    #endregion

    #region Combo Methods

    private void OnHellRipHeadRip(Entity<CanPerformComboComponent> ent, ref HellRipHeadRipPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _)
            || !downed
            || !TryComp(target, out StatusEffectsComponent? status))
            return;

        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 300);
        _damageable.TryChangeDamage(target, damage, true, origin: uid, targetPart: Content.Shared._Shitmed.Targeting.TargetBodyPart.Head);
        var head = _body.GetBodyChildrenOfType(target.Value, BodyPartType.Head).FirstOrDefault();
        if (head != default
            && TryComp<WoundableComponent>(head.Id, out var woundable)
            && woundable.ParentWoundable.HasValue)
            _wound.AmputateWoundable(woundable.ParentWoundable.Value, head.Id, woundable);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnHellRipSlam(Entity<CanPerformComboComponent> ent, ref HellRipSlamPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed)
            || downed
            || !TryComp<PullableComponent>(target, out var pullable))
            return;

        var knockdownTime = TimeSpan.FromSeconds(proto.ParalyzeTime);

        var ev = new BeforeStaminaDamageEvent(1f);
        RaiseLocalEvent(target, ref ev);

        knockdownTime *= ev.Value;

        _stun.TryKnockdown(target, knockdownTime, true, proto.DropHeldItemsBehavior);

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, applyResistances: true);

        _pulling.TryStopPull(target, pullable, ent, true);

        _status.TryRemoveStatusEffect(ent, "KnockedDown");
        _standingState.Stand(ent);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnHellRipDropKick(Entity<CanPerformComboComponent> ent, ref HellRipDropKickPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed)
            || !downed
            || !TryComp<PullableComponent>(target, out var pullable))
            return;

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, applyResistances: true);

        _pulling.TryStopPull(target, pullable, ent, true);
        _grabThrowing.Throw(target, ent, _transform.GetMapCoordinates(ent).Position - _transform.GetMapCoordinates(target).Position, 50);



        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

     private void OnHellRipTearDown(Entity<CanPerformComboComponent> ent,
        ref HellRipTearDownPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !_proto.TryIndex<MartialArtPrototype>(proto.MartialArtsForm.ToString(), out var martialArtProto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed))
            return;

        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"), target);

    }

    #endregion

}


