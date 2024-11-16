using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Bed.Sleep;
using Content.Shared.Body.Part;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Events;
using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Buckle.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared._Shitmed.Medical.Surgery.Conditions;
using Content.Shared._Shitmed.Medical.Surgery.Effects.Step;
using Content.Shared._Shitmed.Medical.Surgery.Steps;
using Content.Shared._Shitmed.Medical.Surgery.Steps.Parts;
using Content.Shared._Shitmed.Medical.Surgery.Tools;
//using Content.Shared.Mood;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared._Shitmed.Body.Part;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed.TypeParsers;
using System.Linq;

namespace Content.Shared._Shitmed.Medical.Surgery;

public abstract partial class SharedSurgerySystem
{
    private static readonly string[] BruteDamageTypes = { "Slash", "Blunt", "Piercing" };
    private static readonly string[] BurnDamageTypes = { "Heat", "Shock", "Cold", "Caustic" };
    private void InitializeSteps()
    {
        SubscribeLocalEvent<SurgeryStepComponent, SurgeryStepEvent>(OnToolStep);
        SubscribeLocalEvent<SurgeryStepComponent, SurgeryStepCompleteCheckEvent>(OnToolCheck);
        SubscribeLocalEvent<SurgeryStepComponent, SurgeryCanPerformStepEvent>(OnToolCanPerform);

        //SubSurgery<SurgeryCutLarvaRootsStepComponent>(OnCutLarvaRootsStep, OnCutLarvaRootsCheck);

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

    private void OnToolStep(Entity<SurgeryStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (ent.Comp.Tool != null)
        {
            foreach (var reg in ent.Comp.Tool.Values)
            {
                if (!AnyHaveComp(args.Tools, reg.Component, out var tool))
                    return;

                if (_net.IsServer &&
                    TryComp(tool, out SurgeryToolComponent? toolComp) &&
                    toolComp.EndSound != null)
                {
                    _audio.PlayEntity(toolComp.EndSound, args.User, tool);
                }
            }
        }

        if (ent.Comp.Add != null)
        {
            foreach (var reg in ent.Comp.Add.Values)
            {
                var compType = reg.Component.GetType();
                if (HasComp(args.Part, compType))
                    continue;
                AddComp(args.Part, _compFactory.GetComponent(compType));
            }
        }

        if (ent.Comp.Remove != null)
        {
            foreach (var reg in ent.Comp.Remove.Values)
            {
                RemComp(args.Part, reg.Component.GetType());
            }
        }

        if (ent.Comp.BodyRemove != null)
        {
            foreach (var reg in ent.Comp.BodyRemove.Values)
            {
                RemComp(args.Body, reg.Component.GetType());
            }
        }

        //if (!HasComp<ForcedSleepingComponent>(args.Body))
        //    //RaiseLocalEvent(args.Body, new MoodEffectEvent("SurgeryPain"));
            // No mood on Goob :(
        if (!_inventory.TryGetSlotEntity(args.User, "gloves", out var _)
        || !_inventory.TryGetSlotEntity(args.User, "mask", out var _))
        {
            var sepsis = new DamageSpecifier(_prototypes.Index<DamageTypePrototype>("Poison"), 5);
            var ev = new SurgeryStepDamageEvent(args.User, args.Body, args.Part, args.Surgery, sepsis, 0.5f);
            RaiseLocalEvent(args.Body, ref ev);
        }
    }

    private void OnToolCheck(Entity<SurgeryStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        // Lord this function is fucking bloated now. Need to clean it up so its less spammy.
        if (ent.Comp.Add != null)
        {
            foreach (var reg in ent.Comp.Add.Values)
            {
                if (!HasComp(args.Part, reg.Component.GetType()))
                {
                    args.Cancelled = true;
                    return;
                }
            }
        }

        if (ent.Comp.Remove != null)
        {
            foreach (var reg in ent.Comp.Remove.Values)
            {
                if (HasComp(args.Part, reg.Component.GetType()))
                {
                    args.Cancelled = true;
                    return;
                }
            }
        }

        if (ent.Comp.BodyRemove != null)
        {
            foreach (var reg in ent.Comp.BodyRemove.Values)
            {
                if (HasComp(args.Body, reg.Component.GetType()))
                {
                    args.Cancelled = true;
                    return;
                }
            }
        }
    }

    private void OnToolCanPerform(Entity<SurgeryStepComponent> ent, ref SurgeryCanPerformStepEvent args)
    {
        if (HasComp<SurgeryOperatingTableConditionComponent>(ent))
        {
            if (!TryComp(args.Body, out BuckleComponent? buckle) ||
                !HasComp<OperatingTableComponent>(buckle.BuckledTo))
            {
                args.Invalid = StepInvalidReason.NeedsOperatingTable;
                return;
            }
        }

        if (_inventory.TryGetContainerSlotEnumerator(args.Body, out var containerSlotEnumerator, args.TargetSlots))
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

        RaiseLocalEvent(args.Body, ref args);

        if (args.Invalid != StepInvalidReason.None)
            return;

        if (ent.Comp.Tool != null)
        {
            args.ValidTools ??= new HashSet<EntityUid>();

            foreach (var reg in ent.Comp.Tool.Values)
            {
                if (!AnyHaveComp(args.Tools, reg.Component, out var withComp))
                {
                    args.Invalid = StepInvalidReason.MissingTool;

                    if (reg.Component is ISurgeryToolComponent tool)
                        args.Popup = $"You need {tool.ToolName} to perform this step!";

                    return;
                }

                args.ValidTools.Add(withComp);
            }
        }
    }

    private EntProtoId? GetProtoId(EntityUid entityUid)
    {
        if (!TryComp<MetaDataComponent>(entityUid, out var metaData))
            return null;

        return metaData.EntityPrototype?.ID;
    }

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

    private void OnTendWoundsStep(Entity<SurgeryTendWoundsEffectComponent> ent, ref SurgeryStepEvent args)
    {
        var group = ent.Comp.MainGroup == "Brute" ? BruteDamageTypes : BurnDamageTypes;

        if (!HasDamageGroup(args.Body, group, out var damageable)
            && !HasDamageGroup(args.Part, group, out var _)
            || damageable == null) // This shouldnt be possible but the compiler doesn't shut up.
            return;


        // Right now the bonus is based off the body's total damage, maybe we could make it based off each part in the future.
        var bonus = ent.Comp.HealMultiplier * damageable.DamagePerGroup[ent.Comp.MainGroup];
        if (_mobState.IsDead(args.Body))
            bonus *= 0.2;

        var adjustedDamage = new DamageSpecifier(ent.Comp.Damage);
        var bonusPerType = bonus / group.Length;

        foreach (var type in group)
        {
            adjustedDamage.DamageDict[type] -= bonusPerType;
        }

        var ev = new SurgeryStepDamageEvent(args.User, args.Body, args.Part, args.Surgery, adjustedDamage, 0.5f);
        RaiseLocalEvent(args.Body, ref ev);
    }

    private void OnTendWoundsCheck(Entity<SurgeryTendWoundsEffectComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        var group = ent.Comp.MainGroup == "Brute" ? BruteDamageTypes : BurnDamageTypes;

        if (HasDamageGroup(args.Body, group, out var _)
            || HasDamageGroup(args.Part, group, out var _))
            args.Cancelled = true;
    }

    /*private void OnCutLarvaRootsStep(Entity<SurgeryCutLarvaRootsStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (TryComp(args.Body, out VictimInfectedComponent? infected) &&
            infected.BurstAt > _timing.CurTime &&
            infected.SpawnedLarva == null)
        {
            infected.RootsCut = true;
        }
    }

    private void OnCutLarvaRootsCheck(Entity<SurgeryCutLarvaRootsStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Body, out VictimInfectedComponent? infected) || !infected.RootsCut)
            args.Cancelled = true;

        // The larva has fully developed and surgery is now impossible
        // TODO: Surgery should still be possible, but the fully developed larva should escape while also saving the hosts life
        if (infected != null && infected.SpawnedLarva != null)
            args.Cancelled = true;
    }*/

    private void OnCavityStep(Entity<SurgeryStepCavityEffectComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Part, out BodyPartComponent? partComp) || partComp.PartType != BodyPartType.Torso)
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
        if (!TryComp(args.Part, out BodyPartComponent? partComp)
            || !TryComp(args.Part, out ItemSlotsComponent? itemComp)
            || ent.Comp.Action == "Insert"
            && !itemComp.Slots[partComp.ContainerName].HasItem
            || ent.Comp.Action == "Remove"
            && itemComp.Slots[partComp.ContainerName].HasItem)
            args.Cancelled = true;
    }

    private void OnAddPartStep(Entity<SurgeryAddPartStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryPartRemovedConditionComponent? removedComp))
            return;

        foreach (var tool in args.Tools)
        {
            if (TryComp(tool, out BodyPartComponent? partComp)
                && partComp.PartType == removedComp.Part
                && (removedComp.Symmetry == null || partComp.Symmetry == removedComp.Symmetry))
            {
                var slotName = removedComp.Symmetry != null
                    ? $"{removedComp.Symmetry?.ToString().ToLower()} {removedComp.Part.ToString().ToLower()}"
                    : removedComp.Part.ToString().ToLower();
                _body.TryCreatePartSlot(args.Part, slotName, partComp.PartType, out var _);
                _body.AttachPart(args.Part, slotName, tool);
                _body.ChangeSlotState((tool, partComp), false);
                EnsureComp<BodyPartReattachedComponent>(tool);
                var ev = new BodyPartAttachedEvent((tool, partComp));
                RaiseLocalEvent(args.Body, ref ev);
            }
        }
    }

    private void OnAffixPartStep(Entity<SurgeryAffixPartStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryPartRemovedConditionComponent? removedComp))
            return;

        var targetPart = _body.GetBodyChildrenOfType(args.Body, removedComp.Part, symmetry: removedComp.Symmetry).FirstOrDefault();

        if (targetPart != default)
        {
            // We reward players for properly affixing the parts by healing a little bit of damage, and enabling the part temporarily.
            var ev = new BodyPartEnableChangedEvent(true);
            RaiseLocalEvent(targetPart.Id, ref ev);
            _damageable.TryChangeDamage(args.Body,
                _body.GetHealingSpecifier(targetPart.Component) * 2,
                canSever: false, // Just in case we heal a brute damage specifier and the logic gets fucky lol
                targetPart: _body.GetTargetBodyPart(targetPart.Component.PartType, targetPart.Component.Symmetry));
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
        if (!TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body != args.Body)
            return;

        var ev = new AmputateAttemptEvent(args.Part);
        RaiseLocalEvent(args.Part, ref ev);
        _hands.TryPickupAnyHand(args.User, args.Part);
    }

    private void OnRemovePartCheck(Entity<SurgeryRemovePartStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body == args.Body)
            args.Cancelled = true;
    }

    private void OnAddOrganStep(Entity<SurgeryAddOrganStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body != args.Body
            || !TryComp(args.Surgery, out SurgeryOrganConditionComponent? organComp)
            || organComp.Organ == null)
            return;

        // Adding organs is generally done for a single one at a time, so we only need to check for the first.
        var firstOrgan = organComp.Organ.Values.FirstOrDefault();
        if (firstOrgan == default)
            return;

        foreach (var tool in args.Tools)
        {
            if (HasComp(tool, firstOrgan.Component.GetType())
                && TryComp<OrganComponent>(tool, out var insertedOrgan)
                && _body.InsertOrgan(args.Part, tool, insertedOrgan.SlotId, partComp, insertedOrgan))
            {
                EnsureComp<OrganReattachedComponent>(tool);
                break;
            }
        }
    }

    private void OnAddOrganCheck(Entity<SurgeryAddOrganStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp<SurgeryOrganConditionComponent>(args.Surgery, out var organComp)
            || organComp.Organ is null
            || !TryComp(args.Part, out BodyPartComponent? partComp)
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
                _body.RemoveOrgan(organs[0].Id, organs[0].Organ);
                _hands.TryPickupAnyHand(args.User, organs[0].Id);
            }
        }
    }

    private void OnRemoveOrganCheck(Entity<SurgeryRemoveOrganStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp<SurgeryOrganConditionComponent>(args.Surgery, out var organComp)
            || organComp.Organ == null
            || !TryComp(args.Part, out BodyPartComponent? partComp)
            || partComp.Body != args.Body)
            return;

        foreach (var reg in organComp.Organ.Values)
        {
            if (_body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var organs)
                && organs != null
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
        foreach (var tool in args.Tools)
        {
            if (TryComp(tool, out MarkingContainerComponent? markingComp)
                && HasComp(tool, organType.Component.GetType()))
            {
                if (!bodyAppearance.MarkingSet.Markings.TryGetValue(markingCategory, out var markingList)
                    || !markingList.Any(marking => marking.MarkingId.Contains(ent.Comp.MatchString)))
                {
                    EnsureComp<BodyPartAppearanceComponent>(args.Part);
                    _body.ModifyMarkings(args.Body, args.Part, bodyAppearance, ent.Comp.MarkingCategory, markingComp.Marking);

                    if (ent.Comp.Accent != null
                        && ent.Comp.Accent.Values.FirstOrDefault() is { } accent)
                    {
                        var compType = accent.Component.GetType();
                        if (!HasComp(args.Body, compType))
                            AddComp(args.Body, _compFactory.GetComponent(compType));
                    }

                    QueueDel(tool); // Again since this isnt actually being inserted we just delete it lol.
                }
            }
        }

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

    private void OnSurgeryTargetStepChosen(Entity<SurgeryTargetComponent> ent, ref SurgeryStepChosenBuiMsg args)
    {
        var user = args.Actor;
        if (GetEntity(args.Entity) is not { Valid: true } body ||
            GetEntity(args.Part) is not { Valid: true } targetPart ||
            !IsSurgeryValid(body, targetPart, args.Surgery, args.Step, user, out var surgery, out var part, out var step))
        {
            return;
        }

        if (!PreviousStepsComplete(body, part, surgery, args.Step) ||
            IsStepComplete(body, part, args.Step, surgery))
            return;

        if (!CanPerformStep(user, body, part, step, true, out _, out _, out var validTools))
            return;

        if (_net.IsServer && validTools?.Count > 0)
        {
            foreach (var tool in validTools)
            {
                if (TryComp(tool, out SurgeryToolComponent? toolComp) &&
                    toolComp.EndSound != null)
                {
                    _audio.PlayEntity(toolComp.StartSound, user, tool);
                }
            }
        }

        if (TryComp(body, out TransformComponent? xform))
            _rotateToFace.TryFaceCoordinates(user, _transform.GetMapCoordinates(body, xform).Position);

        var ev = new SurgeryDoAfterEvent(args.Surgery, args.Step);
        // TODO: Make this serialized on a per surgery step basis, and also add penalties based on ghetto tools.
        var duration = 2f;
        if (TryComp(user, out SurgerySpeedModifierComponent? surgerySpeedMod)
            && surgerySpeedMod is not null)
            duration = duration / surgerySpeedMod.SpeedModifier;

        var doAfter = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(duration), ev, body, part)
        {
            BreakOnMove = true,
            //BreakOnTargetMove = true, I fucking hate wizden dude.
            CancelDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameEvent,
            NeedHand = true,
            BreakOnHandChange = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private (Entity<SurgeryComponent> Surgery, int Step)? GetNextStep(EntityUid body, EntityUid part, Entity<SurgeryComponent?> surgery, List<EntityUid> requirements)
    {
        if (!Resolve(surgery, ref surgery.Comp))
            return null;

        if (requirements.Contains(surgery))
            throw new ArgumentException($"Surgery {surgery} has a requirement loop: {string.Join(", ", requirements)}");

        requirements.Add(surgery);

        if (surgery.Comp.Requirement is { } requirementId &&
            GetSingleton(requirementId) is { } requirement &&
            GetNextStep(body, part, requirement, requirements) is { } requiredNext)
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

    public (Entity<SurgeryComponent> Surgery, int Step)? GetNextStep(EntityUid body, EntityUid part, EntityUid surgery)
    {
        return GetNextStep(body, part, surgery, new List<EntityUid>());
    }

    public bool PreviousStepsComplete(EntityUid body, EntityUid part, Entity<SurgeryComponent> surgery, EntProtoId step)
    {
        // TODO RMC14 use index instead of the prototype id
        if (surgery.Comp.Requirement is { } requirement)
        {
            if (GetSingleton(requirement) is not { } requiredEnt ||
                !TryComp(requiredEnt, out SurgeryComponent? requiredComp) ||
                !PreviousStepsComplete(body, part, (requiredEnt, requiredComp), step))
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

    public bool CanPerformStep(EntityUid user, EntityUid body, EntityUid part,
        EntityUid step, bool doPopup, out string? popup, out StepInvalidReason reason,
        out HashSet<EntityUid>? validTools)
    {
        var type = BodyPartType.Other;
        if (TryComp(part, out BodyPartComponent? partComp))
        {
            type = partComp.PartType;
        }

        var slot = type switch
        {
            BodyPartType.Head => SlotFlags.HEAD,
            BodyPartType.Torso => SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING,
            BodyPartType.Arm => SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING,
            BodyPartType.Hand => SlotFlags.GLOVES,
            BodyPartType.Leg => SlotFlags.OUTERCLOTHING | SlotFlags.LEGS,
            BodyPartType.Foot => SlotFlags.FEET,
            BodyPartType.Tail => SlotFlags.NONE,
            BodyPartType.Other => SlotFlags.NONE,
            _ => SlotFlags.NONE
        };

        var check = new SurgeryCanPerformStepEvent(user, body, GetTools(user), slot);
        RaiseLocalEvent(step, ref check);
        popup = check.Popup;
        validTools = check.ValidTools;

        if (check.Invalid != StepInvalidReason.None)
        {
            if (doPopup && check.Popup != null)
                _popup.PopupEntity(check.Popup, user, user, PopupType.SmallCaution);

            reason = check.Invalid;
            return false;
        }

        reason = default;
        return true;
    }

    public bool CanPerformStep(EntityUid user, EntityUid body, EntityUid part, EntityUid step, bool doPopup)
    {
        return CanPerformStep(user, body, part, step, doPopup, out _, out _, out _);
    }

    public bool IsStepComplete(EntityUid body, EntityUid part, EntProtoId step, EntityUid surgery)
    {
        if (GetSingleton(step) is not { } stepEnt)
            return false;

        var ev = new SurgeryStepCompleteCheckEvent(body, part, surgery);
        RaiseLocalEvent(stepEnt, ref ev);
        return !ev.Cancelled;
    }

    private bool AnyHaveComp(List<EntityUid> tools, IComponent component, out EntityUid withComp)
    {
        foreach (var tool in tools)
        {
            if (HasComp(tool, component.GetType()))
            {
                withComp = tool;
                return true;
            }
        }

        withComp = default;
        return false;
    }
}
