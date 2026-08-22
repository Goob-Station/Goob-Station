// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Pain;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared.Armor;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
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

    private readonly Dictionary<ProtoId<DamageTypePrototype>, List<WoundSeverityCauseEntry>> _woundSeverityCauses = new();
    private readonly List<WoundSeverityCauseEntry> _anyDamageWoundCauses = new();

    private readonly record struct WoundSeverityCauseEntry(ProtoId<TraumaTypePrototype> Id, TraumaTypePrototype Proto, WoundSeverityCause Cause);

    private void InitProcess()
    {
        SubscribeLocalEvent<TraumaInflicterComponent, ComponentInit>(OnTraumaInflicterInit);
        SubscribeLocalEvent<TraumaComponent, ComponentGetState>(OnComponentGet);
        SubscribeLocalEvent<TraumaComponent, ComponentHandleState>(OnComponentHandleState);
        SubscribeLocalEvent<TraumaInflicterComponent, WoundSeverityPointChangedEvent>(OnWoundSeverityPointChanged);
        SubscribeLocalEvent<TraumaInflicterComponent, WoundHealAttemptEvent>(OnWoundHealAttempt);
        SubscribeLocalEvent<WoundableComponent, ApplyTraumaEvent>(OnApplyTrauma);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        BuildWoundSeverityCauseIndex();
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<TraumaTypePrototype>())
            BuildWoundSeverityCauseIndex();
    }

    private void BuildWoundSeverityCauseIndex()
    {
        _woundSeverityCauses.Clear();
        _anyDamageWoundCauses.Clear();

        foreach (var proto in _prototype.EnumeratePrototypes<TraumaTypePrototype>())
        {
            if (proto.Causes == null)
                continue;

            foreach (var cause in proto.Causes)
            {
                if (cause is not WoundSeverityCause wsc)
                    continue;

                var entry = new WoundSeverityCauseEntry(proto.ID, proto, wsc);
                if (wsc.DamageTypes == null || wsc.DamageTypes.Count == 0)
                {
                    _anyDamageWoundCauses.Add(entry);
                    continue;
                }

                foreach (var dt in wsc.DamageTypes)
                {
                    if (!_woundSeverityCauses.TryGetValue(dt, out var list))
                        _woundSeverityCauses[dt] = list = new List<WoundSeverityCauseEntry>();

                    list.Add(entry);
                }
            }
        }
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


            var floor = GetTraumaHealFloor(trauma);
            if (floor == null)
            {
                args.Cancelled = true;
                continue;
            }

            if (floor.Value > args.SeverityFloor)
                args.SeverityFloor = floor.Value;
        }
    }

    private FixedPoint2? GetTraumaHealFloor(Entity<TraumaComponent> trauma)
    {
        if (trauma.Comp.TraumaTarget is not { } target)
            return null;

        if (TryComp<BoneComponent>(target, out var bone))
            return bone.HealSeverityFloor.GetValueOrDefault(bone.BoneSeverity);

        if (TryComp<OrganComponent>(target, out var organ))
            return organ.HealSeverityFloor.GetValueOrDefault(organ.OrganSeverity);

        return null;
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
        if (!Resolve(target, ref woundable, false)
            || !TryComp<WoundComponent>(woundInflicter, out var wound))
            return traumaList;

        var partType = CompOrNull<BodyPartComponent>(target)?.PartType ?? BodyPartType.Other;

        foreach (var entry in _anyDamageWoundCauses)
            TryRollWoundTrauma(entry, target, woundable, woundInflicter, severity, partType, traumaList);

        if (_woundSeverityCauses.TryGetValue(wound.DamageType, out var eligible))
            foreach (var entry in eligible)
                TryRollWoundTrauma(entry, target, woundable, woundInflicter, severity, partType, traumaList);

        return traumaList;
    }

    private void TryRollWoundTrauma(
        WoundSeverityCauseEntry entry,
        EntityUid target,
        WoundableComponent woundable,
        Entity<TraumaInflicterComponent> woundInflicter,
        FixedPoint2 severity,
        BodyPartType partType,
        List<ProtoId<TraumaTypePrototype>> traumaList)
    {
        if (severity < entry.Proto.SeverityGate || !entry.Cause.PartAllowed(partType))
            return;

        var chance = GetWoundTraumaChance(entry, (target, woundable), woundInflicter, severity);
        if (chance > 0 && _random.Prob((float) FixedPoint2.Clamp(chance, 0, 1)))
            traumaList.Add(entry.Id);
    }

    /// <summary>
    /// Runs the cause's <see cref="TraumaChance"/> provider (or the prototype BaseChance fallback),
    /// then applies the generic armour/weapon deduction wrapper.
    /// </summary>
    private FixedPoint2 GetWoundTraumaChance(
        WoundSeverityCauseEntry entry,
        Entity<WoundableComponent> target,
        Entity<TraumaInflicterComponent> woundInflicter,
        FixedPoint2 severity)
    {
        if (entry.Cause.Chance is not { } provider)
            return entry.Proto.BaseChance;

        var bodyPart = Comp<BodyPartComponent>(target);
        if (bodyPart.Body is not { } body)
            return FixedPoint2.Zero;

        var args = new TraumaChanceArgs(EntityManager, target, woundInflicter, severity, bodyPart, body);

        if (provider.Calculate(in args) is not { } baseChance)
            return FixedPoint2.Zero;

        var deduction = GetTraumaChanceDeduction(
            woundInflicter,
            body,
            target,
            Comp<WoundComponent>(woundInflicter).WoundSeverityPoint,
            entry.Id,
            bodyPart.PartType);

        if (deduction == 1)
            return FixedPoint2.Zero;

        return FixedPoint2.Clamp(
            baseChance - deduction + woundInflicter.Comp.TraumasChances.GetValueOrDefault(entry.Id),
            0,
            1);
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

    #region Trauma Application

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

    /// <summary>
    /// Inflicts a trauma that targets a specific organ.
    /// </summary>
    public bool TryInflictOrganTrauma(Entity<WoundableComponent> woundable, EntityUid organ, ProtoId<TraumaTypePrototype> traumaType, FixedPoint2 severity)
    {
        if (_net.IsClient
            || Comp<BodyPartComponent>(woundable).Body == null
            || !_wound.TryInduceWound(woundable, "Blunt", 0f, out var hostWound))
            return false;

        var inflicter = EnsureComp<TraumaInflicterComponent>(hostWound.Value.Owner);
        AddTrauma(organ, woundable, (hostWound.Value.Owner, inflicter), traumaType, severity);
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
