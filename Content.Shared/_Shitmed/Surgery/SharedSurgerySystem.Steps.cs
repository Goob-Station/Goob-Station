// SPDX-FileCopyrightText: 2024 Skubman <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Janet Blackquill <uhhadd@gmail.com>
// SPDX-FileCopyrightText: 2025 Kayzel <43700376+KayzelW@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Trest <144359854+trest100@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 kurokoTurbo <92106367+kurokoTurbo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pacable <77161122+pxc1984@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pacable <igor.mamaev1@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Bed.Sleep;
using Content.Shared.Body.Part;
using Content.Shared.Body.Organ;
using Content.Shared._Shitmed.BodyEffects;
using Content.Shared.Buckle.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared._Shitmed.Medical.Surgery.Conditions;
using Content.Shared._Shitmed.Medical.Surgery.Effects.Step;
using Content.Shared._Shitmed.Medical.Surgery.Steps;
using Content.Shared._Shitmed.Medical.Surgery.Steps.Parts;
using Content.Shared._Shitmed.Medical.Surgery.Tools;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
//using Content.Shared.Mood;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared._Shitmed.Body.Part;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;
using Content.Shared._Shitmed.Surgery;

namespace Content.Shared._Shitmed.Medical.Surgery;

public abstract partial class SharedSurgerySystem
{
    private EntityQuery<BodyPartComponent> _partQuery;
    private EntityQuery<SurgeryIgnoreClothingComponent> _ignoreQuery;
    private EntityQuery<SurgeryStepComponent> _stepQuery;
    private EntityQuery<SurgeryToolComponent> _toolQuery;

    private readonly List<EntityUid> _nextStepList = new();

    private void InitializeSteps()
    {
        _partQuery = GetEntityQuery<BodyPartComponent>();
        _ignoreQuery = GetEntityQuery<SurgeryIgnoreClothingComponent>();
        _stepQuery = GetEntityQuery<SurgeryStepComponent>();
        _toolQuery = GetEntityQuery<SurgeryToolComponent>();

        SubscribeLocalEvent<SurgeryStepComponent, SurgeryStepEvent>(OnToolStep);
        SubscribeLocalEvent<SurgeryStepComponent, SurgeryStepCompleteCheckEvent>(OnToolCheck);
        SubscribeLocalEvent<SurgeryStepComponent, SurgeryCanPerformStepEvent>(OnToolCanPerform);
        SubscribeLocalEvent<SurgeryOperatingTableConditionComponent, SurgeryCanPerformStepEvent>(OnTableCanPerform);

        /*  Abandon all hope ye who enter here. Now I am become shitcoder, the bloater of files.
            On a serious note, I really hate how much bloat this pattern of subscribing to a StepEvent and a CheckEvent
            creates in terms of readability. And while Check DOES only run on the server side, it's still annoying to parse through.*/

        SubSurgery<SurgeryTendWoundsEffectComponent>(OnTendWoundsStep, OnTendWoundsCheck);
        SubSurgery<SurgeryStepCavityEffectComponent>(OnCavityStep, OnCavityCheck);
        SubSurgery<SurgeryAddPartStepComponent>(OnAddPartStep, OnAddPartCheck);
        SubSurgery<SurgeryAffixPartStepComponent>(OnAffixPartStep, OnAffixPartCheck);
        SubSurgery<SurgeryRemovePartStepComponent>(OnRemovePartStep, OnRemovePartCheck);
        SubSurgery<SurgeryAddOrganStepComponent>(OnAddOrganStep, OnAddOrganCheck);
        SubSurgery<SurgeryRemoveOrganStepComponent>(OnRemoveOrganStep, OnRemoveOrganCheck);
        SubSurgery<SurgeryAffixOrganStepComponent>(OnAffixOrganStep, OnAffixOrganCheck);
        SubSurgery<SurgeryAddMarkingStepComponent>(OnAddMarkingStep, OnAddMarkingCheck);
        SubSurgery<SurgeryRemoveMarkingStepComponent>(OnRemoveMarkingStep, OnRemoveMarkingCheck);
        SubSurgery<SurgeryAddOrganSlotStepComponent>(OnAddOrganSlotStep, OnAddOrganSlotCheck);
        SubSurgery<SurgeryTraumaTreatmentStepComponent>(OnTraumaTreatmentStep, OnTraumaTreatmentCheck);
        SubSurgery<SurgeryBleedsTreatmentStepComponent>(OnBleedsTreatmentStep, OnBleedsTreatmentCheck);
        SubSurgery<SurgeryStepPainInflicterComponent>(OnPainInflicterStep, OnPainInflicterCheck);
        Subs.BuiEvents<SurgeryTargetComponent>(SurgeryUIKey.Key, subs =>
        {
            subs.Event<SurgeryStepChosenBuiMsg>(OnSurgeryTargetStepChosen);
        });
    }

    private void SubSurgery<TComp>(EntityEventRefHandler<TComp, SurgeryStepEvent> onStep,
        EntityEventRefHandler<TComp, SurgeryStepCompleteCheckEvent> onComplete) where TComp : IComponent
    {
        SubscribeLocalEvent(onStep);
        SubscribeLocalEvent(onComplete);
    }

    #region Event Methods
    private void OnToolStep(Entity<SurgeryStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryToolAudio(ent, args))
           return;

        AddOrRemoveComponentsToEntity(args.Part, ent.Comp.Add);
        AddOrRemoveComponentsToEntity(args.Part, ent.Comp.Remove, true);
        AddOrRemoveComponentsToEntity(args.Body, ent.Comp.BodyAdd);
        AddOrRemoveComponentsToEntity(args.Body, ent.Comp.BodyRemove,true);

        // Dude this fucking function is so bloated now what the fuck.
        HandleOrganModification(args.Part, args.Body, ent.Comp.AddOrganOnAdd);
        HandleOrganModification(args.Part, args.Body, ent.Comp.RemoveOrganOnAdd,  false);

        HandleSanitization(args);
    }
    private void OnToolCheck(Entity<SurgeryStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (TryToolCheck(ent.Comp.Add, args.Part) ||
            TryToolCheck(ent.Comp.Remove, args.Part, checkMissing: false) ||
            TryToolCheck(ent.Comp.BodyAdd, args.Body) ||
            TryToolCheck(ent.Comp.BodyRemove, args.Body, checkMissing: false))
        {
            args.Cancelled = true;
            return;
        }

        if (TryToolOrganCheck(ent.Comp.AddOrganOnAdd, args.Part)
            || TryToolOrganCheck(ent.Comp.RemoveOrganOnAdd, args.Part, checkMissing: false))
            args.Cancelled = true;
    }

    private void OnToolCanPerform(Entity<SurgeryStepComponent> ent, ref SurgeryCanPerformStepEvent args)
    {
        if (args.IsInvalid)
            return;

        if (!_ignoreQuery.HasComp(args.User)
            && !_ignoreQuery.HasComp(args.Tool)
            && _inventory.TryGetContainerSlotEnumerator(args.Body, out var containerSlotEnumerator, args.TargetSlots))
        {
            while (containerSlotEnumerator.MoveNext(out var containerSlot))
            {
                if (!containerSlot.ContainedEntity.HasValue)
                    continue;

                args.Invalid = StepInvalidReason.Armor;
                args.Popup = Loc.GetString("surgery-ui-window-steps-error-armor");
                return;
            }
        }

        if (ent.Comp.Tool == null)
            return;

        foreach (var reg in ent.Comp.Tool.Values)
        {
            if (GetSurgeryComp(args.Tool, reg.Component) is {} data)
            {
                args.ValidTool = data;
                return; // multiple required tools isn't supported so just return
            }

            args.Invalid = StepInvalidReason.MissingTool;

            if (reg.Component is ISurgeryToolComponent required)
                args.Popup = $"You need {required.ToolName} to perform this step!";
            else
                Log.Error($"Surgery step {ToPrettyString(ent)} wants bad component {reg.Component} which isn't a ISurgeryTool");

            return;
        }
    }

    private void OnTableCanPerform(Entity<SurgeryOperatingTableConditionComponent> ent, ref SurgeryCanPerformStepEvent args)
    {
        if (args.IsInvalid)
            return;

        // mobs that can't be buckled can never be operated because of this check
        if (!TryComp(args.Body, out BuckleComponent? buckle) ||
            !HasComp<OperatingTableComponent>(buckle.BuckledTo))
        {
            args.Invalid = StepInvalidReason.NeedsOperatingTable;
        }
    }

    private string GetDamageGroupByType(string id)
    {
        return (from @group in _prototypes.EnumeratePrototypes<DamageGroupPrototype>() where @group.DamageTypes.Contains(id) select @group.ID).FirstOrDefault()!;
    }

    private void OnTendWoundsStep(Entity<SurgeryTendWoundsEffectComponent> ent, ref SurgeryStepEvent args)
    {
        if (_wounds.GetWoundableSeverityPoint(
                args.Part,
                damageGroup: ent.Comp.MainGroup,
                healable: true) <= 0)
            return;

        // Right now the bonus is based off the body's total damage, maybe we could make it based off each part in the future.
        var bonus = ent.Comp.HealMultiplier * _wounds.GetWoundableSeverityPoint(args.Part, damageGroup: ent.Comp.MainGroup);

        if (_mobState.IsDead(args.Body))
            bonus *= 0.2;

        var adjustedDamage = new DamageSpecifier(ent.Comp.Damage);

        var group = _prototypes.Index<DamageGroupPrototype>(ent.Comp.MainGroup);
        foreach (var type in group.DamageTypes)
            adjustedDamage.DamageDict[type] -= bonus;

        var ev = new SurgeryStepDamageEvent(args.User, args.Body, args.Part, args.Surgery, adjustedDamage, 0.5f);
        RaiseLocalEvent(args.Body, ref ev);
    }

    private void OnTendWoundsCheck(Entity<SurgeryTendWoundsEffectComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (_wounds.HasDamageOfGroup(args.Part, ent.Comp.MainGroup, true))
            args.Cancelled = true;
    }

    private void OnCavityStep(Entity<SurgeryStepCavityEffectComponent> ent, ref SurgeryStepEvent args)
    {
        if (!_partQuery.TryComp(args.Part, out var partComp) || partComp.PartType != BodyPartType.Chest)
            return;

        var activeHandEntity = _hands.EnumerateHeld(args.User).FirstOrDefault();
        if (activeHandEntity != default
            && ent.Comp.Action == "Insert"
            && TryComp(activeHandEntity, out ItemComponent? itemComp)
            && (itemComp.Size.Id == "Tiny"
            || itemComp.Size.Id == "Small"))
            _itemSlotsSystem.TryInsert(ent, partComp.ItemInsertionSlot, activeHandEntity, args.User);
        else if (ent.Comp.Action == "Remove")
            _itemSlotsSystem.TryEjectToHands(ent, partComp.ItemInsertionSlot, args.User);
    }

    private void OnCavityCheck(Entity<SurgeryStepCavityEffectComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        // Normally this check would simply be partComp.ItemInsertionSlot.HasItem, but as mentioned before,
        // For whatever reason it's not instantiating the field on the clientside after the wizmerge.
        if (!_partQuery.TryComp(args.Part, out var partComp)
            || !TryComp(args.Part, out ItemSlotsComponent? itemComp)
            || ent.Comp.Action == "Insert"
            && !itemComp.Slots[partComp.ContainerName].HasItem
            || ent.Comp.Action == "Remove"
            && itemComp.Slots[partComp.ContainerName].HasItem)
            args.Cancelled = true;
    }

    private void OnAddPartStep(Entity<SurgeryAddPartStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryPartRemovedConditionComponent? removedComp)
            || !_partQuery.TryComp(args.Tool, out var partComp)
            || partComp.PartType != removedComp.Part
            || removedComp.Symmetry != null && partComp.Symmetry != removedComp.Symmetry)
            return;

        var slotName = removedComp.Symmetry != null
                ? $"{removedComp.Symmetry?.ToString().ToLower()} {removedComp.Part.ToString().ToLower()}"
                : removedComp.Part.ToString().ToLower();
            _body.TryCreatePartSlot(args.Part, slotName, partComp.PartType, partComp.Symmetry, out var _);
            _body.AttachPart(args.Part, slotName, args.Tool);
            EnsureComp<BodyPartReattachedComponent>(args.Tool);
    }

    private void OnAddOrganSlotStep(Entity<SurgeryAddOrganSlotStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryOrganSlotConditionComponent? condition))
            return;

        _body.TryCreateOrganSlot(args.Part, condition.OrganSlot, out _);
    }

    private void OnAddOrganSlotCheck(Entity<SurgeryAddOrganSlotStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryOrganSlotConditionComponent? condition))
            return;

        args.Cancelled |= !_body.CanInsertOrgan(args.Part, condition.OrganSlot);
    }

    private void OnAffixPartStep(Entity<SurgeryAffixPartStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryPartRemovedConditionComponent? removedComp))
            return;

        var targetPart = _body.GetBodyChildrenOfType(args.Body, removedComp.Part, symmetry: removedComp.Symmetry).FirstOrDefault();

        if (targetPart != default)
        {
            // We reward players for properly affixing the parts by healing a little bit of damage, and enabling the part temporarily.
            _wounds.TryHealWoundsOnWoundable(targetPart.Id, 12f, out _, damageGroup: _prototypes.Index<DamageGroupPrototype>("Brute"));
            RemComp<BodyPartReattachedComponent>(targetPart.Id);
        }
    }

    private void OnAffixPartCheck(Entity<SurgeryAffixPartStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryPartRemovedConditionComponent? removedComp))
            return;

        var targetPart = _body.GetBodyChildrenOfType(args.Body, removedComp.Part, symmetry: removedComp.Symmetry).FirstOrDefault();

        if (targetPart != default
            && HasComp<BodyPartReattachedComponent>(targetPart.Id))
            args.Cancelled = true;
    }

    private void OnAddPartCheck(Entity<SurgeryAddPartStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryPartRemovedConditionComponent? removedComp)
            || !_body.GetBodyChildrenOfType(args.Body, removedComp.Part, symmetry: removedComp.Symmetry).Any())
            args.Cancelled = true;
    }

    private void OnRemovePartStep(Entity<SurgeryRemovePartStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!_partQuery.TryComp(args.Part, out var partComp) || partComp.Body != args.Body)
            return;

        if (!_body.TryGetParentBodyPart(args.Part, out var parentPart, out _))
            return;

        _wounds.AmputateWoundableSafely(parentPart.Value, args.Part, amputateChildrenSafely: true);
        _hands.TryPickupAnyHand(args.User, args.Part);
    }

    private void OnRemovePartCheck(Entity<SurgeryRemovePartStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!_partQuery.TryComp(args.Part, out var partComp) || partComp.Body == args.Body)
            args.Cancelled = true;
    }

    private void OnAddOrganStep(Entity<SurgeryAddOrganStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!_partQuery.TryComp(args.Part, out var partComp)
            || partComp.Body != args.Body
            || !TryComp(args.Surgery, out SurgeryOrganConditionComponent? organComp)
            || organComp.Organ == null)
            return;

        // Adding organs is generally done for a single one at a time, so we only need to check for the first.
        var firstOrgan = organComp.Organ.Values.FirstOrDefault();
        if (firstOrgan == default)
            return;

        if (!HasComp(args.Tool, firstOrgan.Component.GetType())
            || !TryComp<OrganComponent>(args.Tool, out var insertedOrgan)
            || !_body.InsertOrgan(args.Part, args.Tool, insertedOrgan.SlotId, partComp, insertedOrgan))
            return;

        EnsureComp<OrganReattachedComponent>(args.Tool);

        if (insertedOrgan.OriginalBody == args.Body)
            return;

        var ev = new SurgeryStepDamageChangeEvent(args.User, args.Body, args.Part, ent);
            RaiseLocalEvent(ent, ref ev);
            args.Complete = true;
    }

    private void OnAddOrganCheck(Entity<SurgeryAddOrganStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp<SurgeryOrganConditionComponent>(args.Surgery, out var organComp)
            || organComp.Organ is null
            || !_partQuery.TryComp(args.Part, out var partComp)
            || partComp.Body != args.Body)
            return;

        // For now we naively assume that every entity will only have one of each organ type.
        // that we do surgery on, but in the future we'll need to reference their prototype somehow
        // to know if they need 2 hearts, 2 lungs, etc.
        foreach (var reg in organComp.Organ.Values)
        {
            if (!_body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var _))
            {
                args.Cancelled = true;
            }
        }
    }

    private void OnAffixOrganStep(Entity<SurgeryAffixOrganStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryOrganConditionComponent? removedOrganComp)
            || removedOrganComp.Organ == null
            || !removedOrganComp.Reattaching)
            return;

        foreach (var reg in removedOrganComp.Organ.Values)
        {
            _body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var organs);
            if (organs != null && organs.Count > 0)
                RemComp<OrganReattachedComponent>(organs[0].Id);
        }

    }

    private void OnAffixOrganCheck(Entity<SurgeryAffixOrganStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryOrganConditionComponent? removedOrganComp)
            || removedOrganComp.Organ == null
            || !removedOrganComp.Reattaching)
            return;

        foreach (var reg in removedOrganComp.Organ.Values)
        {
            _body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var organs);
            if (organs != null
                && organs.Count > 0
                && organs.Any(organ => HasComp<OrganReattachedComponent>(organ.Id)))
                args.Cancelled = true;
        }
    }

    private void OnRemoveOrganStep(Entity<SurgeryRemoveOrganStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp<SurgeryOrganConditionComponent>(args.Surgery, out var organComp)
            || organComp.Organ == null)
            return;

        foreach (var reg in organComp.Organ.Values)
        {
            _body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var organs);
            if (organs != null && organs.Count > 0)
            {
                if (_body.TryRemoveOrgan(organs[0].Id, organs[0].Organ))
                    _hands.TryPickupAnyHand(args.User, organs[0].Id);
                else
                    _popup.PopupClient(Loc.GetString("surgery-popup-step-SurgeryStepRemoveOrgan-failed"), args.User, args.User);
            }
        }
    }

    private void OnRemoveOrganCheck(Entity<SurgeryRemoveOrganStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp<SurgeryOrganConditionComponent>(args.Surgery, out var organComp)
            || organComp.Organ == null
            || !_partQuery.TryComp(args.Part, out var partComp)
            || partComp.Body != args.Body)
            return;

        foreach (var reg in organComp.Organ.Values)
        {
            if (_body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var organs)
                && organs.Count > 0)
            {
                args.Cancelled = true;
                return;
            }
        }
    }

    // TODO: Refactor bodies to include ears as a prototype instead of doing whatever the hell this is.
    private void OnAddMarkingStep(Entity<SurgeryAddMarkingStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Body, out HumanoidAppearanceComponent? bodyAppearance)
            || ent.Comp.Organ == null)
            return;

        var organType = ent.Comp.Organ.Values.FirstOrDefault();
        if (organType == default)
            return;

        var markingCategory = MarkingCategoriesConversion.FromHumanoidVisualLayers(ent.Comp.MarkingCategory);

        if (!TryComp(args.Tool, out MarkingContainerComponent? markingComp)
            || !HasComp(args.Tool, organType.Component.GetType())
            || bodyAppearance.MarkingSet.Markings.TryGetValue(markingCategory, out var markingList))
            return;

        markingList ??= new List<Marking>();
        if (markingList.Any(marking => marking.MarkingId.Contains(ent.Comp.MatchString)))
            return;

        EnsureComp<BodyPartAppearanceComponent>(args.Part);
        _body.ModifyMarkings(args.Body, args.Part, bodyAppearance, ent.Comp.MarkingCategory, markingComp.Marking);

        if (ent.Comp.Accent != null
            && ent.Comp.Accent.Values.FirstOrDefault() is { } accent)
        {
            var compType = accent.Component.GetType();
            if (!HasComp(args.Body, compType))
                AddComp(args.Body, _compFactory.GetComponent(compType));
        }

        QueueDel(args.Tool); // Again since this isnt actually being inserted we just delete it lol.

    }

    private void OnAddMarkingCheck(Entity<SurgeryAddMarkingStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        var markingCategory = MarkingCategoriesConversion.FromHumanoidVisualLayers(ent.Comp.MarkingCategory);

        if (!TryComp(args.Body, out HumanoidAppearanceComponent? bodyAppearance)
            || !bodyAppearance.MarkingSet.Markings.TryGetValue(markingCategory, out var markingList)
            || !markingList.Any(marking => marking.MarkingId.Contains(ent.Comp.MatchString)))
            args.Cancelled = true;
    }

    private void OnRemoveMarkingStep(Entity<SurgeryRemoveMarkingStepComponent> ent, ref SurgeryStepEvent args)
    {

    }

    private void OnRemoveMarkingCheck(Entity<SurgeryRemoveMarkingStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {

    }

    private void OnTraumaTreatmentStep(Entity<SurgeryTraumaTreatmentStepComponent> ent, ref SurgeryStepEvent args)
    {
        var healAmount = ent.Comp.Amount;
        switch (ent.Comp.TraumaType)
        {
            case TraumaType.OrganDamage:
                foreach (var organ in _body.GetBodyOrgans(args.Body))
                {
                    foreach (var modifier in organ.Component.IntegrityModifiers)
                    {
                        var delta = healAmount - modifier.Value;
                        if (delta > 0)
                        {
                            healAmount -= modifier.Value;
                            _trauma.TryRemoveOrganDamageModifier(
                                organ.Id,
                                modifier.Key.Item2,
                                modifier.Key.Item1,
                                organ.Component);
                        }
                        else
                        {
                            _trauma.TryChangeOrganDamageModifier(
                                organ.Id,
                                -healAmount,
                                modifier.Key.Item2,
                                modifier.Key.Item1,
                                organ.Component);
                            break;
                        }
                    }
                }

                break;

            case TraumaType.BoneDamage:
                if (!TryComp<WoundableComponent>(args.Part, out var woundable))
                    return;

                var bone = woundable.Bone.ContainedEntities.FirstOrNull();
                if (bone == null || !TryComp<BoneComponent>(bone, out var boneComp))
                    return;

                _trauma.ApplyDamageToBone(bone.Value, -healAmount, boneComp);
                break;

            case TraumaType.Dismemberment:
                if (_trauma.TryGetWoundableTrauma(args.Part, out var traumas, TraumaType.Dismemberment))
                    foreach (var trauma in traumas)
                        _trauma.RemoveTrauma(trauma);

                break;
        }
    }

    private void OnTraumaTreatmentCheck(Entity<SurgeryTraumaTreatmentStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (_trauma.HasWoundableTrauma(args.Part, ent.Comp.TraumaType))
            args.Cancelled = true;
    }

    private void OnBleedsTreatmentStep(Entity<SurgeryBleedsTreatmentStepComponent> ent, ref SurgeryStepEvent args)
    {
        var healAmount = ent.Comp.Amount;
        foreach (var woundEnt in _wounds.GetWoundableWounds(args.Part))
        {
            if (!TryComp<BleedInflicterComponent>(woundEnt, out var bleeds))
                continue;

            if (bleeds.Scaling > healAmount)
            {
                bleeds.Scaling -= healAmount;
            }
            else
            {
                bleeds.BleedingAmountRaw = 0;
                bleeds.Scaling = 0;

                bleeds.IsBleeding = false; // Won't bleed as long as it's not reopened

                healAmount -= bleeds.Scaling;
            }

            Dirty(woundEnt, bleeds);
        }
    }

    private void OnBleedsTreatmentCheck(Entity<SurgeryBleedsTreatmentStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        foreach (var woundEnt in _wounds.GetWoundableWounds(args.Part))
        {
            if (!TryComp<BleedInflicterComponent>(woundEnt, out var bleedsInflicter)
                || !bleedsInflicter.IsBleeding)
                continue;

            args.Cancelled = true;
            break;
        }
    }

    private void OnPainInflicterStep(Entity<SurgeryStepPainInflicterComponent> ent, ref SurgeryStepEvent args)
    {
        var ev = new SurgeryPainEvent();
        RaiseLocalEvent(args.Body, ev);
        if (ev.Cancelled)
            return;

        if (!_consciousness.TryGetNerveSystem(args.Body, out var nerveSys))
            return;

        var painToInflict = ent.Comp.Amount;
        if (Status.HasEffectComp<ForcedSleepingStatusEffectComponent>(args.Body))
            painToInflict *= ent.Comp.SleepModifier;

        if (!_pain.TryChangePainModifier(
                nerveSys.Value.Owner,
                args.Part,
                "SurgeryPain",
                painToInflict,
                nerveSys,
                ent.Comp.PainDuration,
                ent.Comp.PainType))
        {
           _pain.TryAddPainModifier(nerveSys.Value.Owner,
               args.Part,
               "SurgeryPain",
               painToInflict,
               ent.Comp.PainType,
               nerveSys,
               ent.Comp.PainDuration);
        }
    }

    private void OnPainInflicterCheck(Entity<SurgeryStepPainInflicterComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!_consciousness.TryGetNerveSystem(args.Part, out var nerveSys))
            return;

        if (!_pain.TryGetPainModifier(nerveSys.Value.Owner, args.Part, "SurgeryPain", out _, nerveSys))
            args.Cancelled = true;
    }


    private void OnSurgeryTargetStepChosen(Entity<SurgeryTargetComponent> ent, ref SurgeryStepChosenBuiMsg args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var user = args.Actor;
        if (GetEntity(args.Entity) is {} body &&
            GetEntity(args.Part) is {} targetPart)
        {
            TryDoSurgeryStep(body, targetPart, user, args.Surgery, args.Step);
        }
    }
    #endregion

    #region Helper Methods
    // I wonder if theres not a function that can do this already.
    private bool HasDamageGroup(EntityUid entity, string[] group, out DamageableComponent? damageable)
    {
        if (!TryComp<DamageableComponent>(entity, out var damageableComp))
        {
            damageable = null;
            return false;
        }

        damageable = damageableComp;
        return group.Any(damageType => damageableComp.Damage.DamageDict.TryGetValue(damageType, out var value) && value > 0);

    }
    private void HandleSanitization(SurgeryStepEvent args)
    {
        if (_inventory.TryGetSlotEntity(args.User, "gloves", out var _)
            && _inventory.TryGetSlotEntity(args.User, "mask", out var _))
            return;

        var sepsisEv = new SurgerySanitizationEvent();
        RaiseLocalEvent(args.User, sepsisEv);
        if (sepsisEv.Handled)
            return;

        if (TryComp<SurgeryTargetComponent>(args.Body, out var surgeryTargetComponent) &&
            surgeryTargetComponent.SepsisImmune)
            return;

        var sepsis = new DamageSpecifier(_prototypes.Index<DamageTypePrototype>("Poison"), 5);
        var ev = new SurgeryStepDamageEvent(args.User, args.Body, args.Part, args.Surgery, sepsis, 0.5f);
        RaiseLocalEvent(args.Body, ref ev);
    }

    private bool TryToolAudio(Entity<SurgeryStepComponent> ent, SurgeryStepEvent args)
    {
        if (ent.Comp.Tool == null)
            return true;

        foreach (var reg in ent.Comp.Tool.Values)
        {
            if (!HasSurgeryComp(args.Tool, reg.Component))
                return false;

            if (_toolQuery.CompOrNull(args.Tool)?.EndSound is {} sound)
            {
                _audio.PlayPredicted(sound, args.Tool, args.User);
                break; // no overlaying sounds
            }
        }

        return true;
    }
    private void HandleOrganModification(EntityUid organTarget,
        EntityUid bodyTarget,
        Dictionary<string, ComponentRegistry>? modifications,
        bool remove = false)
    {
        if (modifications == null)
            return;

        var organSlotIdToOrgan = _body.GetPartOrgans(organTarget).ToDictionary(o => o.Item2.SlotId, o => o);

        foreach (var (slotId, components) in modifications)
        {
            if (!organSlotIdToOrgan.TryGetValue(slotId, out var organValue))
                continue;

            var (organId, organ) = organValue;

            if (remove)
            {
                if (organValue.Item2.OnAdd == null || organ.OnAdd == null)
                    continue;
                RaiseLocalEvent(organId, new OrganComponentsModifyEvent(bodyTarget, false));
                foreach (var key in components.Keys)
                    organ.OnAdd.Remove(key);
            }
            else
            {
                organ.OnAdd ??= new ComponentRegistry();

                foreach (var (key, compToAdd) in components)
                    organ.OnAdd[key] = compToAdd;

                EnsureComp<OrganEffectComponent>(organId);
                RaiseLocalEvent(organId, new OrganComponentsModifyEvent(bodyTarget, true));
            }
        }
    }
    private void AddOrRemoveComponentsToEntity(EntityUid ent, ComponentRegistry? componentRegistry, bool remove = false)
    {
        if(componentRegistry == null)
            return;
        foreach (var reg in componentRegistry.Values)
        {
            var compType = reg.Component.GetType();
            if (remove)
                RemComp(ent, compType);
            else
            {
                if (HasComp(ent, compType))
                    continue;
                AddComp(ent, _compFactory.GetComponent(compType));
            }
        }
    }

    private bool TryToolCheck(ComponentRegistry? components, EntityUid target, bool checkMissing = true)
    {
        if (components == null)
            return false;

        foreach (var (_, entry) in components)
        {
            var hasComponent = HasComp(target, entry.Component.GetType());
            if (checkMissing != hasComponent)
                return true; // Early exit if condition fails
        }

        return false;
    }

    private bool TryToolOrganCheck(IReadOnlyDictionary<string, ComponentRegistry>? organChanges, EntityUid part, bool checkMissing = true)
    {
        if (organChanges == null)
            return false;

        var organSlotIdToOrgan = _body.GetPartOrgans(part).ToDictionary(o => o.Item2.SlotId, o => o.Item2);
        foreach (var (organSlotId, compsToAdd) in organChanges)
        {
            if (!organSlotIdToOrgan.TryGetValue(organSlotId, out var organ))
                continue;

            if (checkMissing)
            {
                if (organ.OnAdd == null || compsToAdd.Keys.Any(key => !organ.OnAdd.ContainsKey(key)))
                {
                    return true;
                }
            }
            else
            {
                if (organ.OnAdd == null)
                    continue;

                if (compsToAdd.Keys.Any(key => organ.OnAdd != null && organ.OnAdd.ContainsKey(key)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryDoSurgeryStep(EntityUid body, EntityUid targetPart, EntityUid user, EntProtoId surgeryId, EntProtoId stepId)
        => TryDoSurgeryStep(body, targetPart, user, surgeryId, stepId, out _);

    /// <summary>
    /// Do a surgery step on a part, if it can be done.
    /// Returns true if it succeeded.
    /// </summary>
    public bool TryDoSurgeryStep(EntityUid body, EntityUid targetPart, EntityUid user, EntProtoId surgeryId, EntProtoId stepId, out StepInvalidReason error)
    {
        error = StepInvalidReason.None;
        if (!IsSurgeryValid(body, targetPart, surgeryId, stepId, user, out var surgery, out var part, out var step))
        {
            error = StepInvalidReason.SurgeryInvalid;
            return false;
        }

        if (!PreviousStepsComplete(body, part, surgery, stepId, user))
        {
            error = StepInvalidReason.MissingPreviousSteps;
            return false;
        }

        if (IsStepComplete(body, part, stepId, surgery))
        {
            error = StepInvalidReason.StepCompleted;
            return false;
        }

        var tool = _hands.GetActiveItemOrSelf(user);
        if (!CanPerformStep(user, body, part, step, tool, true, out _, out error, out var data))
            return false;

        var toolComp = _toolQuery.CompOrNull(tool);
        var usedEv = new SurgeryToolUsedEvent(user, body);
        usedEv.IgnoreToggle = toolComp?.IgnoreToggle ?? false;
        RaiseLocalEvent(tool, ref usedEv);
        if (usedEv.Cancelled)
        {
            error = StepInvalidReason.ToolInvalid;
            return false;
        }

        if (toolComp?.StartSound is {} sound)
            _audio.PlayPredicted(sound, tool, user);

        _rotateToFace.TryFaceCoordinates(user, _transform.GetMapCoordinates(body).Position);

        // We need to check for nullability because of surgeries that dont require a tool, like Cavity Implants
        var speed = data?.Speed ?? 1f;
        var toolUsed = data?.Used ?? false; // if no tool is being used you can't consume it
        var ev = new SurgeryDoAfterEvent(surgeryId, stepId, toolUsed);
        var duration = GetSurgeryDuration(step, user, body, speed);

        if (TryComp(user, out SurgerySpeedModifierComponent? surgerySpeedMod))
            duration = duration / surgerySpeedMod.SpeedModifier;

        var doAfter = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(duration), ev, body, part)
        {
            BreakOnMove = true,
            //BreakOnTargetMove = true, I fucking hate wizden dude.
            CancelDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameEvent,
            NeedHand = true,
            BreakOnHandChange = true,
            AttemptFrequency = AttemptFrequency.EveryTick,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
        {
            error = StepInvalidReason.DoAfterFailed;
            return false;
        }

        var userName = Identity.Entity(user, EntityManager);
        var targetName = Identity.Entity(body, EntityManager);

        var locName = $"surgery-popup-procedure-{surgeryId}-step-{stepId}";
        var locResult = Loc.GetString(locName,
            ("user", userName), ("target", targetName), ("part", part));

        if (locResult == locName)
            locResult = Loc.GetString($"surgery-popup-step-{stepId}",
                ("user", userName), ("target", targetName), ("part", part));

        _popup.PopupPredicted(locResult, user, user);
        return true;
    }

    private float GetSurgeryDuration(EntityUid surgeryStep, EntityUid user, EntityUid target, float toolSpeed)
    {
        if (!_stepQuery.TryComp(surgeryStep, out var stepComp))
            return 2f; // Shouldnt really happen but just a failsafe.

        var speed = toolSpeed;

        if (TryComp(user, out SurgerySpeedModifierComponent? surgerySpeedMod))
            speed *= surgerySpeedMod.SpeedModifier;

        return stepComp.Duration / speed;
    }
    private (Entity<SurgeryComponent> Surgery, int Step)? GetNextStep(EntityUid body, EntityUid part, Entity<SurgeryComponent?> surgery, List<EntityUid> requirements, EntityUid user)
    {
        if (!Resolve(surgery, ref surgery.Comp))
            return null;

        if (requirements.Contains(surgery))
            throw new ArgumentException($"Surgery {surgery} has a requirement loop: {string.Join(", ", requirements)}");


        var ev = new SurgeryIgnorePreviousStepsEvent();
        RaiseLocalEvent(user, ev);
        if (ev.Handled)
        {
            for (var i = surgery.Comp.Steps.Count - 1; i >= 0; i--)
            {
                var surgeryStep = surgery.Comp.Steps[i];
                if (!IsStepComplete(body, part, surgeryStep, surgery))
                    return ((surgery, surgery.Comp), -i - 1);
            }

            return null;
        }

        requirements.Add(surgery);

        if (surgery.Comp.Requirement is { } requirementId &&
            GetSingleton(requirementId) is { } requirement &&
            GetNextStep(body, part, requirement, requirements, user) is { } requiredNext)
        {
            return requiredNext;
        }

        for (var i = 0; i < surgery.Comp.Steps.Count; i++)
        {
            var surgeryStep = surgery.Comp.Steps[i];
            if (!IsStepComplete(body, part, surgeryStep, surgery))
                return ((surgery, surgery.Comp), i);
        }

        return null;
    }

    public (Entity<SurgeryComponent> Surgery, int Step)? GetNextStep(EntityUid body, EntityUid part, EntityUid surgery, EntityUid user)
    {
        _nextStepList.Clear();
        return GetNextStep(body, part, surgery, _nextStepList, user);
    }

    public bool PreviousStepsComplete(EntityUid body, EntityUid part, Entity<SurgeryComponent> surgery, EntProtoId step, EntityUid user)
    {
        var ev = new SurgeryIgnorePreviousStepsEvent();
        RaiseLocalEvent(user, ev);
        if (ev.Handled)
            return true;

        // TODO RMC14 use index instead of the prototype id
        if (surgery.Comp.Requirement is { } requirement)
        {
            if (GetSingleton(requirement) is not { } requiredEnt ||
                !TryComp(requiredEnt, out SurgeryComponent? requiredComp) ||
                !PreviousStepsComplete(body, part, (requiredEnt, requiredComp), step, user))
            {
                return false;
            }
        }

        foreach (var surgeryStep in surgery.Comp.Steps)
        {
            if (surgeryStep == step)
                break;

            if (!IsStepComplete(body, part, surgeryStep, surgery))
                return false;
        }

        return true;
    }

    public bool CanPerformStep(EntityUid user, EntityUid body, EntityUid part, EntityUid step,
        EntityUid tool, bool doPopup, out string? popup, out StepInvalidReason reason,
        out ISurgeryToolComponent? data)
    {
        data = null;

        var type = _partQuery.CompOrNull(part)?.PartType ?? BodyPartType.Other;

        var slot = type switch
        {
            BodyPartType.Head => SlotFlags.HEAD,
            BodyPartType.Chest => SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING,
            BodyPartType.Groin => SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING,
            BodyPartType.Arm => SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING,
            BodyPartType.Hand => SlotFlags.GLOVES,
            BodyPartType.Leg => SlotFlags.OUTERCLOTHING | SlotFlags.LEGS,
            BodyPartType.Foot => SlotFlags.FEET,
            BodyPartType.Tail => SlotFlags.NONE,
            BodyPartType.Other => SlotFlags.NONE,
            _ => SlotFlags.NONE
        };

        var check = new SurgeryCanPerformStepEvent(user, body, tool, slot);
        RaiseLocalEvent(step, ref check);
        if (check.IsValid) // if the step doesn't stop it check the body after
            RaiseLocalEvent(body, ref check);

        popup = check.Popup;
        reason = check.Invalid;
        data = check.ValidTool;

        if (check.IsValid)
            return true;

        if (doPopup && check.Popup != null)
            _popup.PopupClient(check.Popup, user, user, PopupType.SmallCaution);

        return false;
    }

    public bool CanPerformStep(EntityUid user, EntityUid body, EntityUid part, EntityUid step, EntityUid tool, bool doPopup)
    {
        return CanPerformStep(user, body, part, step, tool, doPopup, out _, out _, out _);
    }

    public bool CanPerformStepWithHeld(EntityUid user, EntityUid body, EntityUid part, EntityUid step, bool doPopup, out string? popup)
    {
        var tool = _hands.GetActiveItemOrSelf(user);
        return CanPerformStep(user, body, part, step, tool, doPopup, out popup, out _, out _);
    }

    public bool IsStepComplete(EntityUid body, EntityUid part, EntProtoId step, EntityUid surgery)
    {
        if (GetSingleton(step) is not { } stepEnt)
            return false;

        var ev = new SurgeryStepCompleteCheckEvent(body, part, surgery);
        RaiseLocalEvent(stepEnt, ref ev);
        return !ev.Cancelled;
    }

    private ISurgeryToolComponent? GetSurgeryComp(EntityUid tool, IComponent component)
    {
        if (EntityManager.TryGetComponent(tool, component.GetType(), out var found) && found is ISurgeryToolComponent data)
            return data;

        return null;
    }

    private bool HasSurgeryComp(EntityUid tool, IComponent component) => GetSurgeryComp(tool, component) != null;
    #endregion
}
