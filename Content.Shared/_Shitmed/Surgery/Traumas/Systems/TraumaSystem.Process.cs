// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Pain;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared.Armor;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Damage.Components;
using Content.Shared.Inventory;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Common.SecondSkin;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;

public partial class TraumaSystem
{
    private const string TraumaContainerId = "Traumas";

    private void InitProcess()
    {
        SubscribeLocalEvent<TraumaInflicterComponent, ComponentInit>(OnTraumaInflicterInit);
        SubscribeLocalEvent<TraumaComponent, ComponentGetState>(OnComponentGet);
        SubscribeLocalEvent<TraumaComponent, ComponentHandleState>(OnComponentHandleState);
        SubscribeLocalEvent<TraumaInflicterComponent, WoundSeverityPointChangedEvent>(OnWoundSeverityPointChanged);
        SubscribeLocalEvent<TraumaInflicterComponent, WoundHealAttemptEvent>(OnWoundHealAttempt);
        SubscribeLocalEvent<WoundableComponent, GetTraumaChanceEvent>(OnGetTraumaChance);
        SubscribeLocalEvent<WoundableComponent, ApplyTraumaEvent>(OnApplyTrauma);
    }

    public bool TraumaBlocksHealing(ProtoId<TraumaTypePrototype> traumaType)
    {
        return _prototype.TryIndex(traumaType, out var proto) && proto.BlocksHealing;
    }

    public bool IsTraumaSurgicallyTreatable(ProtoId<TraumaTypePrototype> traumaType)
    {
        return _prototype.TryIndex(traumaType, out var proto) && proto.Treatment is SurgicalTreatment;
    }

    /// <summary>
    /// Collects every surgically-treatable trauma currently on a woundable.
    /// </summary>
    public bool TryGetSurgicallyTreatableTraumas(EntityUid woundable, out List<Entity<TraumaComponent>> traumas, WoundableComponent? woundableComp = null)
    {
        traumas = new List<Entity<TraumaComponent>>();
        if (!Resolve(woundable, ref woundableComp, false))
            return false;

        foreach (var woundEnt in _wound.GetWoundableWounds(woundable, woundableComp))
        {
            if (!TryComp<TraumaInflicterComponent>(woundEnt, out var inflicter))
                continue;

            foreach (var trauma in GetAllWoundTraumas(woundEnt, inflicter))
            {
                if (IsTraumaSurgicallyTreatable(trauma.Comp.TraumaType))
                    traumas.Add(trauma);
            }
        }

        return traumas.Count > 0;
    }

    /// <summary>
    /// Whether a woundable currently carries any trauma that blocks healing.
    /// </summary>
    public bool HasHealingBlockingTrauma(EntityUid woundable, WoundableComponent? woundableComp = null)
    {
        if (!Resolve(woundable, ref woundableComp, false))
            return false;

        foreach (var woundEnt in _wound.GetWoundableWounds(woundable, woundableComp))
        {
            if (!TryComp<TraumaInflicterComponent>(woundEnt, out var inflicter))
                continue;

            foreach (var trauma in GetAllWoundTraumas(woundEnt, inflicter))
            {
                if (!TraumaBlocksHealing(trauma.Comp.TraumaType))
                    continue;

                if (trauma.Comp.TraumaType == BoneDamage
                    && (woundableComp.Bone.ContainedEntities.FirstOrNull() is not { } bone
                        || !TryComp(bone, out BoneComponent? boneComp)
                        || boneComp.BoneSeverity != BoneSeverity.Broken))
                    continue;

                return true;
            }
        }

        return false;
    }

    private void OnTraumaInflicterInit(
        Entity<TraumaInflicterComponent> woundEnt,
        ref ComponentInit args)
    {
        woundEnt.Comp.TraumaContainer = _container.EnsureContainer<Container>(woundEnt, TraumaContainerId);
    }

    private void OnComponentGet(EntityUid uid, TraumaComponent comp, ref ComponentGetState args)
    {
        var state = new TraumaComponentState
        {
            TraumaTarget = GetNetEntity(comp.TraumaTarget),
            HoldingWoundable = GetNetEntity(comp.HoldingWoundable),
            TargetType = comp.TargetType,
            TraumaType = comp.TraumaType,
            TraumaSeverity = comp.TraumaSeverity,
        };

        args.State = state;
    }

    private void OnComponentHandleState(EntityUid uid, TraumaComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not TraumaComponentState state)
            return;

        component.TraumaTarget = GetEntity(state.TraumaTarget);
        component.HoldingWoundable = GetEntity(state.HoldingWoundable);
        component.TargetType = state.TargetType;
        component.TraumaType = state.TraumaType;
        component.TraumaSeverity = state.TraumaSeverity;
    }


    private void OnWoundSeverityPointChanged(
        Entity<TraumaInflicterComponent> woundEnt,
        ref WoundSeverityPointChangedEvent args)
    {
        if (_net.IsClient
            || HasComp<GodmodeComponent>(args.Component.HoldingWoundable))
            return;

        // Overflow is only used when we are capping the wound, so we use it over the computed delta
        // which will be useless in this specific scenario.
        var delta = args.Overflow ?? args.NewSeverity - args.OldSeverity;
        if (delta <= 0 || delta < woundEnt.Comp.SeverityThreshold)
            return;

        var traumasToInduce = RandomTraumaChance(args.Component.HoldingWoundable, woundEnt, delta);
        if (traumasToInduce.Count <= 0)
            return;

        var woundable = args.Component.HoldingWoundable;
        var woundableComp = Comp<WoundableComponent>(args.Component.HoldingWoundable);
        ApplyTraumas((woundable, woundableComp), woundEnt, traumasToInduce, delta);
    }

    private void OnWoundHealAttempt(Entity<TraumaInflicterComponent> inflicter, ref WoundHealAttemptEvent args)
    {
        if (args.IgnoreBlockers)
            return;

        foreach (var trauma in GetAllWoundTraumas(inflicter, inflicter))
        {
            if (!TraumaBlocksHealing(trauma.Comp.TraumaType))
                continue;

            if (trauma.Comp.TraumaType == BoneDamage
                && args.Woundable.Comp.Bone.ContainedEntities.FirstOrNull() is { } bone
                && TryComp(bone, out BoneComponent? boneComp)
                && boneComp.BoneSeverity != BoneSeverity.Broken)
                continue;

            args.Cancelled = true;
        }
    }

    #region Public API

    public IEnumerable<Entity<TraumaComponent>> GetAllWoundTraumas(
        EntityUid woundInflicter,
        TraumaInflicterComponent? component = null)
    {
        if (!Resolve(woundInflicter, ref component, false))
            yield break;

        foreach (var trauma in component.TraumaContainer.ContainedEntities)
        {
            yield return (trauma, Comp<TraumaComponent>(trauma));
        }
    }

    public bool HasAssociatedTrauma(
        EntityUid woundable,
        EntityUid woundInflicter,
        WoundableComponent? woundableComp = null,
        ProtoId<TraumaTypePrototype>? traumaType = null,
        TraumaInflicterComponent? component = null,
        bool showAll = true)
    {
        if (!Resolve(woundInflicter, ref component, false)
            || !Resolve(woundable, ref woundableComp, false))
            return false;

        foreach (var trauma in GetAllWoundTraumas(woundInflicter, component))
        {
            if (trauma.Comp.TraumaTarget == null)
                continue;

            if (trauma.Comp.TraumaType != traumaType && traumaType != null)
                continue;

            if (!showAll)
            {
                // TODO: Fill this with other blocking traumas.
                if (trauma.Comp.TraumaType == BoneDamage
                    && (woundableComp.Bone.ContainedEntities.FirstOrNull() is not { } bone
                    || !TryComp(bone, out BoneComponent? boneComp)
                    || boneComp.BoneSeverity != BoneSeverity.Broken))
                    continue;
            }

            return true;
        }

        return false;
    }

    public bool TryGetAssociatedTrauma(
        EntityUid woundInflicter,
        [NotNullWhen(true)] out List<Entity<TraumaComponent>>? traumas,
        ProtoId<TraumaTypePrototype>? traumaType = null,
        TraumaInflicterComponent? component = null)
    {
        traumas = null;
        if (!Resolve(woundInflicter, ref component, false))
            return false;

        traumas = new List<Entity<TraumaComponent>>();
        foreach (var trauma in GetAllWoundTraumas(woundInflicter, component))
        {
            if (trauma.Comp.TraumaTarget == null)
                continue;

            if (trauma.Comp.TraumaType != traumaType && traumaType != null)
                continue;

            traumas.Add(trauma);
        }

        return true;
    }

    public bool HasWoundableTrauma(
        EntityUid woundable,
        ProtoId<TraumaTypePrototype>? traumaType = null,
        WoundableComponent? woundableComp = null,
        bool showAll = true) // Used to skip certain non-lethal traumas like minor bone fractures.
    {
        if (!Resolve(woundable, ref woundableComp, false))
            return false;

        foreach (var woundEnt in _wound.GetWoundableWounds(woundable, woundableComp))
        {
            if (!TryComp<TraumaInflicterComponent>(woundEnt, out var inflicterComp))
                continue;

            if (HasAssociatedTrauma(woundable, woundEnt, woundableComp, traumaType, inflicterComp, showAll))
                return true;
        }

        return false;
    }

    public bool TryGetWoundableTrauma(
        EntityUid woundable,
        [NotNullWhen(true)] out List<Entity<TraumaComponent>>? traumas,
        ProtoId<TraumaTypePrototype>? traumaType = null,
        WoundableComponent? woundableComp = null)
    {
        traumas = null;
        if (!Resolve(woundable, ref woundableComp, false))
            return false;

        traumas = new List<Entity<TraumaComponent>>();
        foreach (var woundEnt in _wound.GetWoundableWounds(woundable, woundableComp))
        {
            if (!TryComp<TraumaInflicterComponent>(woundEnt, out var inflicterComp))
                continue;

            if (TryGetAssociatedTrauma(woundEnt, out var traumasFound, traumaType, inflicterComp))
                traumas.AddRange(traumasFound);
        }

        return traumas.Count > 0;
    }

    public bool HasBodyTrauma(
        EntityUid body,
        ProtoId<TraumaTypePrototype>? traumaType = null,
        BodyComponent? bodyComp = null)
    {
        return Resolve(body, ref bodyComp, false) && _body.GetBodyChildren(body, bodyComp).Any(bodyPart => HasWoundableTrauma(bodyPart.Id, traumaType));
    }

    public bool TryGetBodyTraumas(
        EntityUid body,
        [NotNullWhen(true)] out List<Entity<TraumaComponent>>? traumas,
        ProtoId<TraumaTypePrototype>? traumaType = null,
        BodyComponent? bodyComp = null)
    {
        traumas = null;
        if (!Resolve(body, ref bodyComp, false))
            return false;

        traumas = new List<Entity<TraumaComponent>>();
        foreach (var bodyPart in _body.GetBodyChildren(body, bodyComp))
        {
            if (TryGetWoundableTrauma(bodyPart.Id, out var traumasFound, traumaType))
                traumas.AddRange(traumasFound);
        }

        return traumas.Count > 0;
    }

    public List<ProtoId<TraumaTypePrototype>> RandomTraumaChance(
        EntityUid target,
        Entity<TraumaInflicterComponent> woundInflicter,
        FixedPoint2 severity,
        WoundableComponent? woundable = null)
    {
        var traumaList = new List<ProtoId<TraumaTypePrototype>>();
        if (!Resolve(target, ref woundable, false))
            return traumaList;

        foreach (var typeId in woundInflicter.Comp.AllowedTraumas)
        {
            if (!_prototype.TryIndex(typeId, out var proto) || severity < proto.SeverityGate)
                continue;

            var ev = new GetTraumaChanceEvent(typeId, (target, woundable), woundInflicter, severity);
            RaiseLocalEvent(target, ref ev);

            var chance = ev.Handled ? ev.Chance : proto.BaseChance;
            if (chance > 0 && _random.Prob((float) FixedPoint2.Clamp(chance, 0, 1)))
                traumaList.Add(typeId);
        }

        return traumaList;
    }

    private void OnGetTraumaChance(Entity<WoundableComponent> ent, ref GetTraumaChanceEvent args)
    {
        // TODO: NUKE OLD CHANCE HANDLERS AND MAKE ALL DAMAGES INTO TRAUMAS
        FixedPoint2? chance = null;
        if (args.TraumaType == BoneDamage)
            chance = BoneTraumaChance(args.Woundable, args.Inflicter);
        else if (args.TraumaType == NerveDamage)
            chance = NerveDamageChance(args.Woundable, args.Inflicter);
        else if (args.TraumaType == OrganDamage)
            chance = OrganTraumaChance(args.Woundable, args.Inflicter);
        else if (args.TraumaType == Dismemberment)
            chance = DismembermentTraumaChance(args.Woundable, args.Inflicter);

        if (chance == null)
            return;

        args.Chance = chance.Value;
        args.Handled = true;
    }

    public FixedPoint2 GetArmourChanceDeduction(EntityUid body, Entity<TraumaInflicterComponent> inflicter, ProtoId<TraumaTypePrototype> traumaType, BodyPartType coverage)
    {
        var deduction = FixedPoint2.Zero;

        var ev = new GetSecondSkinDeductionEvent((int) coverage, traumaType);
        RaiseLocalEvent(body, ref ev);
        deduction += ev.Deduction;

        foreach (var ent in _inventory.GetHandOrInventoryEntities(body, SlotFlags.WITHOUT_POCKET))
        {
            if (!TryComp<ArmorComponent>(ent, out var armour)
                || !armour.TraumaDeductions.TryGetValue(traumaType, out var traumaDeduction))
                continue;

            if (!inflicter.Comp.AllowArmourDeduction.Contains(traumaType) && traumaDeduction >= 0)
                continue;

            if (armour.ArmorCoverage.Contains(coverage))
                deduction += traumaDeduction;
        }

        return deduction;
    }

    public FixedPoint2 GetTraumaChanceDeduction(
        Entity<TraumaInflicterComponent> inflicter,
        EntityUid body,
        Entity<WoundableComponent> traumaTarget,
        FixedPoint2 severity,
        ProtoId<TraumaTypePrototype> traumaType,
        BodyPartType coverage)
    {
        var deduction = traumaTarget.Comp.TraumaDeductions.GetValueOrDefault(traumaType, FixedPoint2.Zero);
        deduction += GetArmourChanceDeduction(body, inflicter, traumaType, coverage);

        var traumaDeductionEvent = new TraumaChanceDeductionEvent(severity, traumaType, 0);
        RaiseLocalEvent(traumaTarget, ref traumaDeductionEvent);

        deduction += traumaDeductionEvent.ChanceDeduction;

        return deduction;
    }

    public void ApplyMangledTraumas(EntityUid woundable,
        EntityUid wound,
        FixedPoint2 severity,
        WoundableComponent? woundableComp = null,
        TraumaInflicterComponent? inflicterComponent = null)
    {
        if (!Resolve(wound, ref inflicterComponent, false)
            || !Resolve(woundable, ref woundableComp, false)
            || inflicterComponent.MangledMultipliers == null)
            return;

        var traumasToInduce = new List<ProtoId<TraumaTypePrototype>>();
        foreach (var traumaType in inflicterComponent.MangledMultipliers.Keys)
        {
            if (!_prototype.TryIndex(traumaType, out var proto))
                continue;

            // TODO: NUKE OLD CHANCE HANDLERS AND MAKE ALL DAMAGES INTO TRAUMAS
            if (proto.Target == TraumaTarget.Bone
                && (woundableComp.Bone.ContainedEntities.FirstOrNull() is not { } bone
                    || !HasComp<BoneComponent>(bone)))
                continue;

            traumasToInduce.Add(traumaType);
        }

        ApplyTraumas((woundable, woundableComp), (wound, inflicterComponent), traumasToInduce, severity);
    }

    #endregion

    #region Trauma Chance Randoming

    public FixedPoint2 BoneTraumaChance(Entity<WoundableComponent> target, Entity<TraumaInflicterComponent> woundInflicter)
    {
        var bodyPart = Comp<BodyPartComponent>(target);
        if (!bodyPart.Body.HasValue)
            return FixedPoint2.Zero; // Can't sever if already severed

        var bone = target.Comp.Bone.ContainedEntities.FirstOrNull();

        if (bone == null || !TryComp<BoneComponent>(bone, out var boneComp))
            return FixedPoint2.Zero;

        if (boneComp.BoneSeverity == BoneSeverity.Broken)
            return FixedPoint2.Zero;

        var deduction = GetTraumaChanceDeduction(
            woundInflicter,
            bodyPart.Body.Value,
            target,
            Comp<WoundComponent>(woundInflicter).WoundSeverityPoint,
            BoneDamage,
            bodyPart.PartType);

        if (deduction == 1)
            return FixedPoint2.Zero;

        // We do complete random to get the chance for trauma to happen,
        // We combine multiple parameters and do some math, to get the chance.
        // Even if we get 0.1 damage there's still a chance for injury to be applied, but with the extremely low chance.
        // The more damage, the bigger is the chance.
        return FixedPoint2.Clamp(
            target.Comp.IntegrityCap / (target.Comp.WoundableIntegrity + boneComp.BoneIntegrity)
             * _boneTraumaChanceMultipliers[target.Comp.WoundableSeverity]
             - deduction + woundInflicter.Comp.TraumasChances.GetValueOrDefault(BoneDamage),
            0,
            1);
    }

    public FixedPoint2 NerveDamageChance(
        Entity<WoundableComponent> target,
        Entity<TraumaInflicterComponent> woundInflicter)
    {
        var bodyPart = Comp<BodyPartComponent>(target);
        if (!bodyPart.Body.HasValue)
            return FixedPoint2.Zero; // No entity to apply pain to

        if (!TryComp<NerveComponent>(target, out var nerve))
            return FixedPoint2.Zero;

        if (nerve.PainFeels < 0.2)
            return FixedPoint2.Zero;

        var deduction = GetTraumaChanceDeduction(
            woundInflicter,
            bodyPart.Body.Value,
            target,
            Comp<WoundComponent>(woundInflicter).WoundSeverityPoint,
            NerveDamage,
            bodyPart.PartType);

        if (deduction == 1)
            return FixedPoint2.Zero;
        // literally dismemberment chance, but lower by default
        return FixedPoint2.Clamp(
                target.Comp.WoundableIntegrity / target.Comp.IntegrityCap / 20
                - deduction + woundInflicter.Comp.TraumasChances.GetValueOrDefault(NerveDamage),
                0,
                1);
    }

    public FixedPoint2 OrganTraumaChance(
        Entity<WoundableComponent> target,
        Entity<TraumaInflicterComponent> woundInflicter)
    {
        var bodyPart = Comp<BodyPartComponent>(target);
        if (!bodyPart.Body.HasValue)
            return FixedPoint2.Zero; // No entity to apply pain to

        var totalIntegrity =
            _body.GetPartOrgans(target, bodyPart)
                .Aggregate(FixedPoint2.Zero, (current, organ) => current + organ.Component.OrganIntegrity);

        if (totalIntegrity <= 0) // No surviving organs
            return FixedPoint2.Zero;

        var deduction = GetTraumaChanceDeduction(
            woundInflicter,
            bodyPart.Body.Value,
            target,
            Comp<WoundComponent>(woundInflicter).WoundSeverityPoint,
            OrganDamage,
            bodyPart.PartType);

        if (deduction == 1)
            return FixedPoint2.Zero;
        // organ damage is like, very deadly, but not yet
        // so like, like, yeah, we don't want a disabler to induce some EVIL ASS organ damage with a 0,000001% chance and ruin your round
        // Very unlikely to happen if your woundables are in a good condition

        return FixedPoint2.Clamp(
                target.Comp.WoundableIntegrity / target.Comp.IntegrityCap / totalIntegrity
                - deduction + woundInflicter.Comp.TraumasChances.GetValueOrDefault(OrganDamage),
                0,
                1);
    }

    public FixedPoint2 DismembermentTraumaChance(
        Entity<WoundableComponent> target,
        Entity<TraumaInflicterComponent> woundInflicter)
    {
        var bodyPart = Comp<BodyPartComponent>(target);
        if (!bodyPart.Body.HasValue)
            return FixedPoint2.Zero; // Can't sever if already severed

        var parentWoundable = target.Comp.ParentWoundable;
        if (!parentWoundable.HasValue)
            return FixedPoint2.Zero;

        if (bodyPart.PartType == BodyPartType.Chest
            || (bodyPart.PartType == BodyPartType.Groin
            && Comp<WoundableComponent>(parentWoundable.Value).WoundableSeverity != WoundableSeverity.Mangled))
            return FixedPoint2.Zero;

        var deduction = GetTraumaChanceDeduction(
            woundInflicter,
            bodyPart.Body.Value,
            target,
            Comp<WoundComponent>(woundInflicter).WoundSeverityPoint,
            Dismemberment,
            bodyPart.PartType);

        if (deduction == 1)
            return FixedPoint2.Zero;

        var bonePenalty = FixedPoint2.New(1); // higher means less chance to delimb
        if (TryComp<BonelessComponent>(target.Owner, out var bonelessComp))
            bonePenalty = bonelessComp.BonePenalty;

        // Healthy bones decrease the chance of your limb getting delimbed
        var bone = target.Comp.Bone.ContainedEntities.FirstOrNull();
        var multiplier = 1f;
        if (bone != null && TryComp<BoneComponent>(bone, out var boneComp))
        {
            switch (boneComp.BoneSeverity)
            {
                case BoneSeverity.Normal:
                    multiplier *= 0.3f; // decreases delimb chance by 70%
                    break;
                case BoneSeverity.Damaged:
                    multiplier *= 0.6f; // 40%
                    break;
                case BoneSeverity.Cracked:
                    multiplier *= 1f; // 0%
                    break;
                case BoneSeverity.Broken:
                    multiplier *= 1.2f; // increases by 20%
                    break;
                default:
                    break;
            }
        }

        return FixedPoint2.Clamp(
                (1f - (MathF.Pow(target.Comp.WoundableIntegrity.Float(), 1.3f) / target.Comp.IntegrityCap - 1f) * bonePenalty) * multiplier
                - deduction + woundInflicter.Comp.TraumasChances.GetValueOrDefault(Dismemberment),
                0,
                1);
    }

    public EntityUid AddTrauma(
        EntityUid target,
        Entity<WoundableComponent> holdingWoundable,
        Entity<TraumaInflicterComponent> inflicter,
        ProtoId<TraumaTypePrototype> traumaType,
        FixedPoint2 severity,
        (BodyPartType, BodyPartSymmetry)? targetType = null)
    {
        if (TerminatingOrDeleted(inflicter))
            return EntityUid.Invalid;

        foreach (var trauma in inflicter.Comp.TraumaContainer.ContainedEntities)
        {
            var containedTraumaComp = Comp<TraumaComponent>(trauma);
            if (containedTraumaComp.TraumaType != traumaType
                || containedTraumaComp.TraumaTarget != target)
                continue;
            // Check for TraumaTarget isn't really necessary..
            // Right now wounds on a specified woundable can't wound other woundables, but in case IF something happens or IF someone decides to do that

            //  Allows us to create multiple dismemberment traumas on the same body part.
            if (targetType.HasValue
                && targetType.Value != containedTraumaComp.TargetType)
                continue;

            containedTraumaComp.TraumaSeverity = severity;
            Dirty(trauma, containedTraumaComp);
            return trauma;
        }

        var entityProto = inflicter.Comp.TraumaPrototypes.TryGetValue(traumaType, out var overrideProto)
            ? overrideProto
            : _prototype.Index(traumaType).TraumaEntity;
        var traumaEnt = Spawn(entityProto);
        var traumaComp = EnsureComp<TraumaComponent>(traumaEnt);

        traumaComp.TraumaSeverity = severity;

        traumaComp.TraumaTarget = target;

        if (targetType.HasValue)
            traumaComp.TargetType = targetType.Value;

        traumaComp.HoldingWoundable = holdingWoundable;

        _container.Insert(traumaEnt, inflicter.Comp.TraumaContainer);

        var evTrauma = new TraumaInducedEvent((traumaEnt, traumaComp), target, severity, traumaType);
        RaiseLocalEvent(traumaEnt, ref evTrauma);

        // Raise the event on the woundable
        var ev = new TraumaInducedEvent((traumaEnt, traumaComp), target, severity, traumaType);
        RaiseLocalEvent(holdingWoundable, ref ev);

        // Raise the event on the inflicter (wound)
        var ev1 = new TraumaInducedEvent((traumaEnt, traumaComp), target, severity, traumaType);
        RaiseLocalEvent(inflicter, ref ev1);

        Dirty(traumaEnt, traumaComp);
        return traumaEnt;
    }

    public void RemoveTrauma(
        Entity<TraumaComponent> trauma)
    {
        if (!_container.TryGetContainingContainer((trauma.Owner, Transform(trauma.Owner), MetaData(trauma.Owner)), out var traumaContainer))
            return;

        if (!TryComp<TraumaInflicterComponent>(traumaContainer.Owner, out var traumaInflicter))
            return;

        RemoveTrauma(trauma, (traumaContainer.Owner, traumaInflicter));
    }

    public void RemoveTrauma(
        Entity<TraumaComponent> trauma,
        Entity<TraumaInflicterComponent> inflicterWound)
    {
        _container.Remove(trauma.Owner, inflicterWound.Comp.TraumaContainer, reparent: false, force: true);

        if (trauma.Comp.TraumaTarget != null)
        {
            var evTrauma = new TraumaBeingRemovedEvent(trauma, trauma.Comp.TraumaTarget.Value, trauma.Comp.TraumaSeverity, trauma.Comp.TraumaType);
            RaiseLocalEvent(trauma.Owner, ref evTrauma);

            var ev = new TraumaBeingRemovedEvent(trauma, trauma.Comp.TraumaTarget.Value, trauma.Comp.TraumaSeverity, trauma.Comp.TraumaType);
            RaiseLocalEvent(inflicterWound, ref ev);

            if (trauma.Comp.HoldingWoundable != null)
            {
                var ev1 = new TraumaBeingRemovedEvent(trauma, trauma.Comp.TraumaTarget.Value, trauma.Comp.TraumaSeverity, trauma.Comp.TraumaType);
                RaiseLocalEvent(trauma.Comp.HoldingWoundable.Value, ref ev1);
            }
        }

        if (_net.IsServer)
            QueueDel(trauma);
    }

    #endregion

    #region Private API

    /// <summary>
    /// Inflicts a trauma from an external cause (explosion, etc.) that has no existing wound to host it.
    /// </summary>
    public bool TryInflictTrauma(Entity<WoundableComponent> woundable, ProtoId<TraumaTypePrototype> traumaType, FixedPoint2 severity)
    {
        if (_net.IsClient
            || Comp<BodyPartComponent>(woundable).Body == null
            || !_wound.TryInduceWound(woundable, "Blunt", 0f, out var hostWound))
            return false;

        var inflicter = EnsureComp<TraumaInflicterComponent>(hostWound.Value.Owner);
        ApplyTrauma(woundable, (hostWound.Value.Owner, inflicter), traumaType, severity);
        return true;
    }

    private void ApplyTraumas(Entity<WoundableComponent> target, Entity<TraumaInflicterComponent> inflicter, List<ProtoId<TraumaTypePrototype>> traumas, FixedPoint2 severity)
    {
        if (_net.IsClient
            || !Comp<BodyPartComponent>(target).Body.HasValue)
            return;

        foreach (var traumaType in traumas)
            ApplyTrauma(target, inflicter, traumaType, severity);
    }

    /// <summary>
    /// Selects the target sub-entity for a single trauma, runs the before/apply events.
    /// </summary>
    private void ApplyTrauma(Entity<WoundableComponent> target, Entity<TraumaInflicterComponent> inflicter, ProtoId<TraumaTypePrototype> traumaType, FixedPoint2 severity)
    {
        if (!_prototype.TryIndex(traumaType, out var proto))
            return;

        var targetChosen = SelectTraumaTarget(target, proto.Target);
        if (targetChosen == null)
            return;

        var beforeTraumaInduced = new BeforeTraumaInducedEvent(severity, targetChosen.Value, traumaType);
        RaiseLocalEvent(target, ref beforeTraumaInduced);
        if (beforeTraumaInduced.Cancelled)
            return;

        var apply = new ApplyTraumaEvent(traumaType, target, inflicter, targetChosen.Value, severity);
        RaiseLocalEvent(target, ref apply);

        if (!apply.Handled)
            AddTrauma(targetChosen.Value, target, inflicter, traumaType, severity);

        // TODO: veins, would have been very lovely to integrate this into vascular system
        // TODO: FUCK YOURSELF
        //if (RandomVeinsTraumaChance(woundable))
        //{
        //    traumaApplied = ApplyDamageToVeins(woundable.Veins!.ContainedEntities[0], severity * _veinsDamageMultipliers[woundable.WoundableSeverity]);
        //    _sawmill.Info(traumaApplied
        //        ? $"A new trauma (Raw Severity: {severity}) was created on target: {target} of type Vein damage"
        //        : $"Tried to create a trauma on target: {target}, but no trauma was applied. Type: Vein damage.");
        //}
    }

    private EntityUid? SelectTraumaTarget(Entity<WoundableComponent> woundable, TraumaTarget target)
    {
        return target switch
        {
            TraumaTarget.Bone => woundable.Comp.Bone.ContainedEntities.FirstOrNull(),
            TraumaTarget.Organ => SelectRandomOrgan(woundable),
            TraumaTarget.ParentWoundable => woundable.Comp.ParentWoundable,
            TraumaTarget.Woundable => woundable.Owner,
            _ => null,
        };
    }

    private EntityUid? SelectRandomOrgan(Entity<WoundableComponent> woundable)
    {
        var organs = _body.GetPartOrgans(woundable).ToList();
        if (organs.Count == 0)
            return null;

        _random.Shuffle(organs);
        return organs[0].Id;
    }

    /// <summary>
    /// Applies the old trauma behaviours.
    /// TODO: NUKE TS AS WELL, FUCK YOU MOCHO
    /// </summary>
    private void OnApplyTrauma(Entity<WoundableComponent> ent, ref ApplyTraumaEvent args)
    {
        var target = args.Woundable;
        var inflicter = args.Inflicter;
        var severity = args.Severity;
        var targetChosen = args.Target;
        var bodyPart = Comp<BodyPartComponent>(target);

        if (args.TraumaType == BoneDamage)
        {
            if (ApplyBoneTrauma(targetChosen, target, inflicter, severity)
                && bodyPart.Body is { } bodyBone
                && _consciousness.TryGetNerveSystem(bodyBone, out var nerveBone))
            {
                _pain.TryAddPainModifier(
                    nerveBone.Value.Owner,
                    target.Owner,
                    "BoneDamage",
                    severity / 1.4f,
                    PainDamageTypes.TraumaticPain,
                    nerveBone.Value.Comp);
            }

            args.Handled = true;
        }
        else if (args.TraumaType == OrganDamage)
        {
            var traumaEnt = AddTrauma(targetChosen, target, inflicter, OrganDamage, severity);

            if (traumaEnt != EntityUid.Invalid
                && !TryChangeOrganDamageModifier(targetChosen, severity, traumaEnt, "WoundableDamage"))
                TryCreateOrganDamageModifier(targetChosen, severity, traumaEnt, "WoundableDamage");

            args.Handled = true;
        }
        else if (args.TraumaType == NerveDamage)
        {
            if (bodyPart.Body is { } bodyNerve
                && _consciousness.TryGetNerveSystem(bodyNerve, out var nerveNerve))
            {
                var time = TimeSpan.FromSeconds((float) severity * 2.4);

                // Fooling people into thinking they have no pain.
                // 10 (raw pain) * 1.4 (multiplier) = 14 (actual pain)
                // 1 - 0.28 = 0.72 (the fraction of pain the person feels)
                // 14 * 0.72 = 10.08 (the pain the player can actually see) ... Barely noticeable :3
                // KYS
                _pain.TryAddPainMultiplier(nerveNerve.Value, "NerveDamage", 1.4f, time: time);
                _pain.TryAddPainFeelsModifier(nerveNerve.Value, "NerveDamage", target, -0.28f, time: time);

                foreach (var child in _wound.GetAllWoundableChildren(target))
                {
                    _pain.TryAddPainFeelsModifier(nerveNerve.Value, "NerveDamage", child, -0.7f, time: time);
                }
            }

            args.Handled = true;
        }
        else if (args.TraumaType == Dismemberment)
        {
            if (!_wound.IsWoundableRoot(target)
                && _wound.TryInduceWound(targetChosen, "Blunt", 0f, out var woundInduced)) // We need this to add the trauma into.
            {
                AddTrauma(
                    targetChosen,
                    (targetChosen, Comp<WoundableComponent>(targetChosen)),
                    (woundInduced.Value.Owner, EnsureComp<TraumaInflicterComponent>(woundInduced.Value.Owner)),
                    Dismemberment,
                    severity,
                    (bodyPart.PartType, bodyPart.Symmetry));

                _wound.AmputateWoundable(targetChosen, target, target);
            }

            args.Handled = true;
        }
    }

    #endregion
}
