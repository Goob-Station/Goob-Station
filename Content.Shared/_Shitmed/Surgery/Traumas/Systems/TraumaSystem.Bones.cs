// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.DoAfter;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Weapons.Melee.Events;
using Content.Shared._Shitmed.Weapons.Ranged.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Robust.Shared.Audio;
using Robust.Shared.Utility;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;

public partial class TraumaSystem
{
    private void InitBones()
    {
        SubscribeLocalEvent<BoneComponent, BoneSeverityChangedEvent>(OnBoneSeverityChanged);
        SubscribeLocalEvent<BoneComponent, BoneIntegrityChangedEvent>(OnBoneIntegrityChanged);
        SubscribeLocalEvent<BoneComponent, GetDoAfterDelayMultiplierEvent>(OnGetDoAfterDelayMultiplier);
        SubscribeLocalEvent<BoneComponent, AttemptHandsMeleeEvent>(OnAttemptHandsMelee);
        SubscribeLocalEvent<BoneComponent, AttemptHandsShootEvent>(OnAttemptHandsShoot);
    }

    #region Event Handling

    private void OnBoneSeverityChanged(Entity<BoneComponent> bone, ref BoneSeverityChangedEvent args)
    {
        if (bone.Comp.BoneWoundable == null
            || args.NewSeverity < args.OldSeverity)
            return;

        var bodyComp = Comp<BodyPartComponent>(bone.Comp.BoneWoundable.Value);

        if (!bodyComp.Body.HasValue)
            return;

        var part = bodyComp.ParentSlot is null
            ? bodyComp.PartType.ToString().ToLower()
            : bodyComp.ParentSlot.Value.Id;

        _popup.PopupClient(Loc.GetString($"popup-trauma-BoneDamage-{args.NewSeverity.ToString()}", ("part", part)),
            bodyComp.Body.Value,
            PopupType.SmallCaution);

        var volumeFloat = args.NewSeverity switch
        {
            BoneSeverity.Damaged => -8f,
            BoneSeverity.Cracked => 1f,
            BoneSeverity.Broken => 6f,
            _ => 0f,
        };

        _audio.PlayPvs(bone.Comp.BoneBreakSound, bodyComp.Body.Value, AudioParams.Default.WithVolume(volumeFloat));
    }

    private void OnBoneIntegrityChanged(Entity<BoneComponent> bone, ref BoneIntegrityChangedEvent args)
    {
        if (bone.Comp.BoneWoundable == null)
            return;

        var bodyComp = Comp<BodyPartComponent>(bone.Comp.BoneWoundable.Value);
        if (!bodyComp.Body.HasValue)
            return;

        if (args.NewIntegrity == bone.Comp.IntegrityCap)
        {
            if (bodyComp.PartType == BodyPartType.Hand)
                _virtual.DeleteInHandsMatching(bodyComp.Body.Value, bone);

            if (TryGetWoundableTrauma(bone.Comp.BoneWoundable.Value, out var traumas, TraumaType.BoneDamage))
                foreach (var trauma in traumas.Where(trauma => trauma.Comp.TraumaTarget == bone))
                    RemoveTrauma(trauma);
        }

        switch (bodyComp.PartType)
        {
            case BodyPartType.Leg:
            case BodyPartType.Foot:
                ProcessLegsState(bodyComp.Body.Value);

                break;
        }
    }

    private void OnGetDoAfterDelayMultiplier(Entity<BoneComponent> bone, ref GetDoAfterDelayMultiplierEvent args)
    {
        args.Multiplier *= bone.Comp.BoneSeverity switch
        {
            BoneSeverity.Damaged => 0.92f,
            BoneSeverity.Cracked => 0.84f,
            BoneSeverity.Broken => 0.75f,
            _ => 1f,
        };
    }

    private void OnAttemptHandsMelee(Entity<BoneComponent> bone, ref AttemptHandsMeleeEvent args)
    {
        var odds = bone.Comp.BoneSeverity switch
        {
            BoneSeverity.Cracked => 0.10f,
            BoneSeverity.Broken => 0.25f,
            _ => 0f,
        };

        if (odds == 0f
            || args.Handled
            || bone.Comp.BoneWoundable is null
            || !TryComp(bone.Comp.BoneWoundable.Value, out BodyPartComponent? bodyPart)
            || bodyPart.Body is not { } body)
            return;

        if (TryFumble("arm-fumble", new SoundPathSpecifier("/Audio/Effects/slip.ogg"), body, odds))
        {
            args.Handled = true;
            args.Cancel();
        }
    }

    private void OnAttemptHandsShoot(Entity<BoneComponent> bone, ref AttemptHandsShootEvent args)
    {
        var odds = bone.Comp.BoneSeverity switch
        {
            BoneSeverity.Cracked => 0.10f,
            BoneSeverity.Broken => 0.25f,
            _ => 0f,
        };

        if (odds == 0f
            || args.Handled
            || bone.Comp.BoneWoundable is null
            || !TryComp(bone.Comp.BoneWoundable.Value, out BodyPartComponent? bodyPart)
            || bodyPart.Body is not { } body)
            return;

        if (TryFumble("arm-fumble", new SoundPathSpecifier("/Audio/Effects/slip.ogg"), body, odds))
            args.Handled = true;
    }

    #endregion

    #region Public API

    public bool ApplyDamageToBone(EntityUid bone, FixedPoint2 severity, BoneComponent? boneComp = null)
    {
        if (severity == 0
            || !Resolve(bone, ref boneComp))
            return false;

        var newIntegrity = FixedPoint2.Clamp(boneComp.BoneIntegrity - severity, 0, boneComp.IntegrityCap);
        if (boneComp.BoneIntegrity == newIntegrity)
            return false;

        var ev = new BoneIntegrityChangedEvent((bone, boneComp), boneComp.BoneIntegrity, newIntegrity);
        RaiseLocalEvent(bone, ref ev);

        boneComp.BoneIntegrity = newIntegrity;
        CheckBoneSeverity(bone, boneComp);

        Dirty(bone, boneComp);
        return true;
    }

    public bool ApplyBoneTrauma(
        EntityUid boneEnt,
        Entity<WoundableComponent> woundable,
        Entity<TraumaInflicterComponent> inflicter,
        FixedPoint2 inflicterSeverity,
        BoneComponent? boneComp = null)
    {
        if (!Resolve(boneEnt, ref boneComp))
            return false;

        if (_net.IsServer)
            AddTrauma(boneEnt, woundable, inflicter, TraumaType.BoneDamage, inflicterSeverity);

        ApplyDamageToBone(boneEnt, inflicterSeverity, boneComp);

        return true;
    }

    public bool SetBoneIntegrity(EntityUid bone, FixedPoint2 integrity, BoneComponent? boneComp = null)
    {
        if (!Resolve(bone, ref boneComp))
            return false;

        var newIntegrity = FixedPoint2.Clamp(integrity, 0, boneComp.IntegrityCap);
        if (boneComp.BoneIntegrity == newIntegrity)
            return false;

        var ev = new BoneIntegrityChangedEvent((bone, boneComp), boneComp.BoneIntegrity, newIntegrity);
        RaiseLocalEvent(bone, ref ev);

        boneComp.BoneIntegrity = newIntegrity;
        CheckBoneSeverity(bone, boneComp);

        Dirty(bone, boneComp);
        return true;
    }

    /// <summary>
    /// Updates the broken bones alert for a body based on its current bone state
    /// </summary>
    public void UpdateBodyBoneAlert(EntityUid body, BodyComponent? bodyComp = null)
    {
        if (!Resolve(body, ref bodyComp))
            return;

        bool hasBrokenBones = false;

        var rootPart = bodyComp.RootContainer.ContainedEntity;
        if (rootPart.HasValue)
        {
            foreach (var (_, woundable) in _wound.GetAllWoundableChildren(rootPart.Value))
            {
                if (woundable.Bone == null)
                    continue;

                foreach (var boneEntity in woundable.Bone.ContainedEntities)
                {
                    if (!TryComp(boneEntity, out BoneComponent? boneComp))
                        continue;

                    if (boneComp.BoneSeverity == BoneSeverity.Broken)
                    {
                        hasBrokenBones = true;
                        break;
                    }
                }

                if (hasBrokenBones)
                    break;
            }
        }

        // Update the alert based on whether any bones are broken
        if (hasBrokenBones)
            _alert.ShowAlert(body, _brokenBonesAlertId);
        else
            _alert.ClearAlert(body, _brokenBonesAlertId);
    }

    #endregion

    #region Private API

    private void CheckBoneSeverity(EntityUid bone, BoneComponent boneComp)
    {
        var nearestSeverity = boneComp.BoneSeverity;

        foreach (var (severity, value) in _boneThresholds.OrderByDescending(kv => kv.Value))
        {
            if (boneComp.BoneIntegrity < value)
                continue;

            nearestSeverity = severity;
            break;
        }

        if (nearestSeverity != boneComp.BoneSeverity)
        {
            var ev = new BoneSeverityChangedEvent((bone, boneComp), boneComp.BoneSeverity, nearestSeverity);
            RaiseLocalEvent(bone, ref ev, true);
        }

        boneComp.BoneSeverity = nearestSeverity;
        Dirty(bone, boneComp);

        if (boneComp.BoneWoundable != null
            && TryComp<BodyPartComponent>(boneComp.BoneWoundable.Value, out var bodyPartComp)
            && bodyPartComp.Body is { } body)
            UpdateBodyBoneAlert(body);
    }


    private void ProcessLegsState(EntityUid body, BodyComponent? bodyComp = null)
    {
        if (!Resolve(body, ref bodyComp))
            return;

        var rawWalkSpeed = 0f; // just used to compare to actual speed values
        var walkSpeed = 0f;
        var sprintSpeed = 0f;
        var acceleration = 0f;

        foreach (var legEntity in bodyComp.LegEntities)
        {
            if (!TryComp<MovementBodyPartComponent>(legEntity, out var movement))
                continue;

            var partWalkSpeed = movement.WalkSpeed;
            var partSprintSpeed = movement.SprintSpeed;
            var partAcceleration = movement.Acceleration;

            if (!TryComp<WoundableComponent>(legEntity, out var legWoundable))
                continue;

            if (!TryComp<BoneComponent>(legWoundable.Bone.ContainedEntities.First(), out var boneComp))
                continue;

            // Get the foot penalty
            var penalty = 1f;
            var footEnt =
                _body.GetBodyChildrenOfType(body,
                        BodyPartType.Foot,
                        symmetry: Comp<BodyPartComponent>(legEntity).Symmetry)
                    .FirstOrNull();

            if (footEnt != null)
            {
                if (TryComp<BoneComponent>(legWoundable.Bone.ContainedEntities.FirstOrNull(), out var footBone))
                {
                    penalty = footBone.BoneSeverity switch
                    {
                        BoneSeverity.Damaged => 0.77f,
                        BoneSeverity.Cracked => 0.66f,
                        BoneSeverity.Broken => 0.55f,
                        _ => penalty,
                    };
                }
            }
            else
            {
                // You are supposed to have one
                penalty = 0.44f;
            }

            rawWalkSpeed += partWalkSpeed;
            partWalkSpeed *= penalty;
            partSprintSpeed *= penalty;
            partAcceleration *= penalty;

            switch (boneComp.BoneSeverity)
            {
                case BoneSeverity.Cracked:
                    walkSpeed += partWalkSpeed / 2f;
                    sprintSpeed += partSprintSpeed / 2f;
                    acceleration += partAcceleration / 2f;
                    break;

                case BoneSeverity.Damaged:
                    walkSpeed += partWalkSpeed / 1.6f;
                    sprintSpeed += partSprintSpeed / 1.6f;
                    acceleration += partAcceleration / 1.6f;
                    break;

                case BoneSeverity.Normal:
                    walkSpeed += partWalkSpeed;
                    sprintSpeed += partSprintSpeed;
                    acceleration += partAcceleration;
                    break;
            }
        }

        rawWalkSpeed /= bodyComp.RequiredLegs;
        walkSpeed /= bodyComp.RequiredLegs;
        sprintSpeed /= bodyComp.RequiredLegs;
        acceleration /= bodyComp.RequiredLegs;

        _movementSpeed.ChangeBaseSpeed(body, walkSpeed, sprintSpeed, acceleration);

        if (walkSpeed < rawWalkSpeed / 3.4)
            _standing.Down(body);
    }

    private bool TryFumble(string message, SoundPathSpecifier sound, EntityUid body, float odds)
    {
        var rand = new System.Random((int) _timing.CurTick.Value);
        if (rand.NextFloat() < odds)
        {
            _popup.PopupClient(Loc.GetString(message), body, PopupType.Medium);
            var ev = new DropHandItemsEvent();
            RaiseLocalEvent(body, ref ev, false);
            _audio.PlayPredicted(sound, body, body);
            return true;
        }
        return false;
    }

    #endregion
}
