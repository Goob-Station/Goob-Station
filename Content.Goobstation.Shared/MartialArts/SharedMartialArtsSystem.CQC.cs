// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
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
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Standing;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.MartialArts;

public partial class SharedMartialArtsSystem
{
    private static readonly SoundSpecifier NeckSnapSound = new SoundPathSpecifier("/Audio/_Goobstation/Wounds/crack1.ogg");

    private void InitializeCqc()
    {
        SubscribeLocalEvent<CanPerformComboComponent, CqcSlamPerformedEvent>(OnCQCSlam);
        SubscribeLocalEvent<CanPerformComboComponent, CqcKickPerformedEvent>(OnCQCKick);
        SubscribeLocalEvent<CanPerformComboComponent, CqcRestrainPerformedEvent>(OnCQCRestrain);
        SubscribeLocalEvent<CanPerformComboComponent, CqcPressurePerformedEvent>(OnCQCPressure);
        SubscribeLocalEvent<CanPerformComboComponent, CqcConsecutivePerformedEvent>(OnCQCConsecutive);

        SubscribeLocalEvent<GrantCqcComponent, UseInHandEvent>(OnGrantCQCUse);
        SubscribeLocalEvent<GrantCqcComponent, MapInitEvent>(OnMapInitEvent);
        SubscribeLocalEvent<GrantCqcComponent, ExaminedEvent>(OnGrantCQCExamine);
    }

    #region Generic Methods

        private void OnMapInitEvent(Entity<GrantCqcComponent> ent, ref MapInitEvent args)
        {
            if (!HasComp<MobStateComponent>(ent))
                return;

            if (!TryGrantMartialArt(ent, ent.Comp))
                return;

            if (TryComp<MartialArtsKnowledgeComponent>(ent, out var knowledge))
                knowledge.Blocked = true;
        }

    private void OnGrantCQCUse(EntityUid ent, GrantMartialArtKnowledgeComponent comp, UseInHandEvent args)
    {
        if (!_netManager.IsServer)
            return;

        if (comp.Used)
        {
            _popupSystem.PopupEntity(Loc.GetString(comp.LearnFailMessage, ("manual", Identity.Entity(ent, EntityManager))),
                args.User,
                args.User);
            return;
        }

        if (!TryGrantMartialArt(args.User, comp))
            return;
        _popupSystem.PopupEntity(Loc.GetString(comp.LearnMessage), args.User, args.User);
        comp.Used = true;
    }

    private void OnGrantCQCExamine(EntityUid ent, GrantMartialArtKnowledgeComponent comp, ExaminedEvent args)
    {
        if (comp.Used)
            args.PushMarkup(Loc.GetString("cqc-manual-used", ("manual", Identity.Entity(ent, EntityManager))));
    }

    private void OnCQCAttackPerformed(Entity<MartialArtsKnowledgeComponent> ent, ref ComboAttackPerformedEvent args)
    {
        if (args.Weapon != args.Performer || args.Target == args.Performer)
            return;

        switch (args.Type)
        {
            case ComboAttackType.Disarm:
                _stamina.TakeStaminaDamage(args.Target, 25f, applyResistances: true);
                break;
            case ComboAttackType.Harm:
                // Snap neck
                if (!_mobState.IsDead(args.Target) && !HasComp<GodmodeComponent>(args.Target) &&
                    TryComp(ent, out PullerComponent? puller) && puller.Pulling == args.Target &&
                    TryComp(args.Target, out PullableComponent? pullable) &&
                    puller.GrabStage == GrabStage.Suffocate && TryComp(ent, out TargetingComponent? targeting) &&
                    targeting.Target == TargetBodyPart.Head)
                {
                    _pulling.TryStopPull(args.Target, pullable);
                    _mobState.ChangeMobState(args.Target, MobState.Dead, null, ent);

                    var dmg = new DamageSpecifier
                    {
                        DamageDict =
                        {
                            { "Blunt", 100 },
                            { "Asphyxiation", 300 },
                        },
                    };

                    _damageable.TryChangeDamage(args.Target,
                        dmg,
                        true,
                        canSever: false,
                        origin: ent,
                        targetPart: TargetBodyPart.Head);

                    if (_netManager.IsServer)
                        _audio.PlayPvs(NeckSnapSound, args.Target);

                    ComboPopup(ent, args.Target, "Neck Snap");
                    break;
                }

                // Leg sweep
                if (!TryComp<StandingStateComponent>(ent, out var standing)
                    || standing.CurrentState == StandingState.Standing ||
                    !TryComp(args.Target, out StandingStateComponent? targetStanding) ||
                    targetStanding.CurrentState != StandingState.Standing)
                    break;

                _status.TryRemoveStatusEffect(ent, "KnockedDown");
                _standingState.Stand(ent);
                _stun.TryKnockdown(args.Target, TimeSpan.FromSeconds(5), true);
                ComboPopup(ent, args.Target, "Leg Sweep");
                break;
        }
    }

    #endregion

    #region Combo Methods

    private void OnCQCSlam(Entity<CanPerformComboComponent> ent, ref CqcSlamPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed)
            || downed)
            return;

        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(proto.ParalyzeTime), true, proto.DropHeldItemsBehavior);
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
    }

    private void OnCQCKick(Entity<CanPerformComboComponent> ent, ref CqcKickPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed))
            return;

        var mapPos = _transform.GetMapCoordinates(ent).Position;
        var hitPos = _transform.GetMapCoordinates(target).Position;
        var dir = hitPos - mapPos;
        dir *= 1f / dir.Length();

        if (downed)
        {
            if (TryComp<StaminaComponent>(target, out var stamina) && stamina.Critical)
                _status.TryAddStatusEffect<ForcedSleepingComponent>(target, "ForcedSleep", TimeSpan.FromSeconds(10), true);
            DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _, TargetBodyPart.Head);
            _stamina.TakeStaminaDamage(target, proto.StaminaDamage * 2 + 5, source: ent, applyResistances: true);
        }
        else
        {
            _stamina.TakeStaminaDamage(target, proto.StaminaDamage, source: ent, applyResistances: true);
        }

        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _grabThrowing.Throw(target, ent, dir, proto.ThrownSpeed);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit2.ogg"), target);
        ComboPopup(ent, target, proto.Name);
    }

    private void OnCQCRestrain(Entity<CanPerformComboComponent> ent, ref CqcRestrainPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        _stun.TryKnockdown(target, TimeSpan.FromSeconds(proto.ParalyzeTime), true, proto.DropHeldItemsBehavior);
        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, source: ent, applyResistances: true);
        ComboPopup(ent, target, proto.Name);
    }

    private void OnCQCPressure(Entity<CanPerformComboComponent> ent, ref CqcPressurePerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, source: ent, applyResistances: true);

        ComboPopup(ent, target, proto.Name);

        if (!_hands.TryGetActiveItem(target, out var activeItem))
            return;
        if(!_hands.TryDrop(target, activeItem.Value))
            return;
        if (!_hands.TryGetEmptyHand(ent, out var emptyHand))
            return;
        if(!_hands.TryPickup(ent, activeItem.Value, emptyHand))
            return;
        _hands.SetActiveHand(ent, emptyHand);
    }

    private void OnCQCConsecutive(Entity<CanPerformComboComponent> ent, ref CqcConsecutivePerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);
        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, source: ent, applyResistances: true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"), target);
        ComboPopup(ent, target, proto.Name);
    }

    #endregion
}
