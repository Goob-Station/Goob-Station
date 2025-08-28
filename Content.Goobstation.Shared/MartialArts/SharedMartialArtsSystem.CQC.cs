// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 Baptr0b0t <152836416+Baptr0b0t@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Baptr0b0t <152836416+baptr0b0t@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lincoln McQueen <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Bed.Sleep;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Standing;
using Robust.Shared.Audio;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.MartialArts;

public partial class SharedMartialArtsSystem
{
    private void InitializeCqc()
    {
        SubscribeLocalEvent<CanPerformComboComponent, CqcSlamPerformedEvent>(OnCQCSlam);
        SubscribeLocalEvent<CanPerformComboComponent, CqcKickPerformedEvent>(OnCQCKick);
        SubscribeLocalEvent<CanPerformComboComponent, CqcRestrainPerformedEvent>(OnCQCRestrain);
        SubscribeLocalEvent<CanPerformComboComponent, CqcPressurePerformedEvent>(OnCQCPressure);
        SubscribeLocalEvent<CanPerformComboComponent, CqcConsecutivePerformedEvent>(OnCQCConsecutive);

        SubscribeLocalEvent<MartialArtsKnowledgeComponent, CanDoCQCEvent>(OnCQCCheck);

        SubscribeLocalEvent<GrantCqcComponent, UseInHandEvent>(OnGrantCQCUse);
        SubscribeLocalEvent<GrantCqcComponent, MapInitEvent>(OnMapInitEvent);
    }

    #region Generic Methods

    private void OnCQCCheck(Entity<MartialArtsKnowledgeComponent> ent, ref CanDoCQCEvent args)
    {
        if (args.Handled)
            return;

        if (!ent.Comp.Blocked)
        {
            args.Handled = true;
            return;
        }

        foreach (var entInRange in _lookup.GetEntitiesInRange(ent, 8f))
        {
            if (!TryPrototype(entInRange, out var entProto) || entProto.ID != "SpawnPointChef")
                continue;

            args.Handled = true;
            return;
        }
    }

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
        if (args.Handled)
            return;

        args.Handled = true;

        if (!_netManager.IsServer)
            return;

        if (!TryGrantMartialArt(args.User, comp))
            return;

        var coords = Transform(args.User).Coordinates;
        _audio.PlayPvs(comp.SoundOnUse, coords);
        if (comp.MultiUse)
            return;

        QueueDel(ent);
        if (comp.SpawnedProto == null)
            return;

        Spawn(comp.SpawnedProto, coords);
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
                    TryComp(args.Target, out BodyComponent? body) &&
                    TryComp(args.Target, out StaminaComponent? stamina) && stamina.Critical &&
                    puller.GrabStage == GrabStage.Suffocate && TryComp(ent, out TargetingComponent? targeting) &&
                    targeting.Target == TargetBodyPart.Head
                    && _mobThreshold.TryGetDeadThreshold(args.Target, out var damageToKill))
                {
                    _pulling.TryStopPull(args.Target, pullable);

                    var blunt = new DamageSpecifier(_proto.Index<DamageTypePrototype>("Blunt"), damageToKill.Value);
                    _damageable.TryChangeDamage(args.Target, blunt, true, targetPart: TargetBodyPart.Chest);

                    var (partType, symmetry) = _body.ConvertTargetBodyPart(targeting.Target);
                    var targetedBodyPart = _body.GetBodyChildrenOfType(args.Target, partType, body, symmetry)
                        .ToList()
                        .FirstOrNull();

                    if (targetedBodyPart == null ||
                        !TryComp(targetedBodyPart.Value.Id, out WoundableComponent? woundable) ||
                        woundable.Bone.ContainedEntities.FirstOrNull() is not { } bone ||
                        !TryComp(bone, out BoneComponent? boneComp) || boneComp.BoneSeverity == BoneSeverity.Broken)
                        break;

                    _trauma.ApplyDamageToBone(bone, boneComp.BoneIntegrity, boneComp);
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
        ent.Comp.LastAttacks.Clear();
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
        ent.Comp.LastAttacks.Clear();
    }

    private void OnCQCRestrain(Entity<CanPerformComboComponent> ent, ref CqcRestrainPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        _stun.TryKnockdown(target, TimeSpan.FromSeconds(proto.ParalyzeTime), true, proto.DropHeldItemsBehavior);
        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, source: ent, applyResistances: true);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnCQCPressure(Entity<CanPerformComboComponent> ent, ref CqcPressurePerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, source: ent, applyResistances: true);

        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();

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
        ent.Comp.LastAttacks.Clear();
    }

    #endregion
}
