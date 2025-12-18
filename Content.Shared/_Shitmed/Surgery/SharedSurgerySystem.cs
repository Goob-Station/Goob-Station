// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 AstroDogeDX <48888500+AstroDogeDX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Janet Blackquill <uhhadd@gmail.com>
// SPDX-FileCopyrightText: 2025 Kayzel <43700376+KayzelW@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Trest <144359854+trest100@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 kurokoTurbo <92106367+kurokoTurbo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Shitmed.Medical.Surgery.Conditions;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Steps;
using Content.Shared._Shitmed.Medical.Surgery.Steps.Parts;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Surgery;
using Content.Shared.Buckle.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Prototypes;
using Content.Shared.Stacks;
using Content.Shared.Standing;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Body.Organ;

namespace Content.Shared._Shitmed.Medical.Surgery;

public abstract partial class SharedSurgerySystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly RotateToFaceSystem _rotateToFace = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly WoundSystem _wounds = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;
    [Dependency] private readonly ConsciousnessSystem _consciousness = default!;
    [Dependency] private readonly PainSystem _pain = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] protected readonly StatusEffectsSystem Status = default!;

    private EntityQuery<BodyComponent> _bodyQuery;
    private EntityQuery<StackComponent> _stackQuery;

    /// <summary>
    /// Cache of all surgery prototypes' singleton entities.
    /// Cleared after a prototype reload.
    /// </summary>
    private readonly Dictionary<EntProtoId, EntityUid> _surgeries = new();

    private readonly List<EntProtoId> _allSurgeries = new();

    /// <summary>
    /// Every surgery entity prototype id.
    /// Kept in sync with prototype reloads.
    /// </summary>
    public IReadOnlyList<EntProtoId> AllSurgeries => _allSurgeries;

    public override void Initialize()
    {
        base.Initialize();

        _bodyQuery = GetEntityQuery<BodyComponent>();
        _stackQuery = GetEntityQuery<StackComponent>();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);

        SubscribeLocalEvent<SurgeryTargetComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SurgeryTargetComponent, DoAfterAttemptEvent<SurgeryDoAfterEvent>>(OnBeforeTargetDoAfter);
        SubscribeLocalEvent<SurgeryTargetComponent, SurgeryDoAfterEvent>(OnTargetDoAfter);
        SubscribeLocalEvent<SurgeryCloseIncisionConditionComponent, SurgeryValidEvent>(OnCloseIncisionValid);
        SubscribeLocalEvent<SurgeryHasBodyConditionComponent, SurgeryValidEvent>(OnHasBodyConditionValid);
        SubscribeLocalEvent<SurgeryPartConditionComponent, SurgeryValidEvent>(OnPartConditionValid);
        SubscribeLocalEvent<SurgeryOrganConditionComponent, SurgeryValidEvent>(OnOrganConditionValid);
        SubscribeLocalEvent<SurgeryWoundedConditionComponent, SurgeryValidEvent>(OnWoundedValid);
        SubscribeLocalEvent<SurgeryPartRemovedConditionComponent, SurgeryValidEvent>(OnPartRemovedConditionValid);
        SubscribeLocalEvent<SurgeryBodyConditionComponent, SurgeryValidEvent>(OnBodyConditionValid);
        SubscribeLocalEvent<SurgeryOrganSlotConditionComponent, SurgeryValidEvent>(OnOrganSlotConditionValid);
        SubscribeLocalEvent<SurgeryPartPresentConditionComponent, SurgeryValidEvent>(OnPartPresentConditionValid);
        SubscribeLocalEvent<SurgeryTraumaPresentConditionComponent, SurgeryValidEvent>(OnTraumaPresentConditionValid);
        SubscribeLocalEvent<SurgeryBleedsPresentConditionComponent, SurgeryValidEvent>(OnBleedsPresentConditionValid);
        SubscribeLocalEvent<SurgeryMarkingConditionComponent, SurgeryValidEvent>(OnMarkingPresentValid);
        SubscribeLocalEvent<SurgeryBodyComponentConditionComponent, SurgeryValidEvent>(OnBodyComponentConditionValid);
        SubscribeLocalEvent<SurgeryPartComponentConditionComponent, SurgeryValidEvent>(OnPartComponentConditionValid);
        SubscribeLocalEvent<SurgeryOrganOnAddConditionComponent, SurgeryValidEvent>(OnOrganOnAddConditionValid);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
        SubscribeLocalEvent<SanitizedComponent, SurgerySanitizationEvent>(OnSanitization);
        SubscribeLocalEvent<SanitizedComponent, HeldRelayedEvent<SurgerySanitizationEvent>>(OnHeldSanitization);

        InitializeSteps();
        InitializeStart();

        LoadPrototypes();
    }

    private void OnHeldSanitization(Entity<SanitizedComponent> ent, ref HeldRelayedEvent<SurgerySanitizationEvent> args)
    {
        if (ent.Comp.WorksInHands)
            args.Args.Handled = true;
    }

    private void OnSanitization(Entity<SanitizedComponent> ent, ref SurgerySanitizationEvent args)
    {
        args.Handled = true;
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _surgeries.Clear();
    }

    private void OnMapInit(Entity<SurgeryTargetComponent> ent, ref MapInitEvent args)
    {
        var data = new InterfaceData("SurgeryBui");
        _ui.SetUi(ent.Owner, SurgeryUIKey.Key, data);
    }

    private void OnBeforeTargetDoAfter(Entity<SurgeryTargetComponent> ent,
        ref DoAfterAttemptEvent<SurgeryDoAfterEvent> args)
    {
        if (_net.IsClient
            || !args.Event.Repeat) // We only wanna do this laggy shit on repeatables. One-time stuff idc.
            return;

        if (args.Event.Target is not { } target
            || !IsSurgeryValid(ent, target, args.Event.Surgery, args.Event.Step, args.Event.User, out var surgery, out var part, out var _)
            || IsStepComplete(ent, part, args.Event.Step, surgery))
            args.Cancel();
    }

    private void OnTargetDoAfter(Entity<SurgeryTargetComponent> ent, ref SurgeryDoAfterEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (args.Cancelled)
        {
            var failEv = new SurgeryStepFailedEvent(args.User, ent, args.Surgery, args.Step);
            RaiseLocalEvent(args.User, ref failEv);
            return;
        }

        var tool = _hands.GetActiveItemOrSelf(args.User);
        if (args.Handled
            || args.Target is not { } target
            || !IsSurgeryValid(ent, target, args.Surgery, args.Step, args.User, out var surgery, out var part, out var step)
            || !PreviousStepsComplete(ent, part, surgery, args.Step, args.User)
            || !CanPerformStep(args.User, ent, part, step, tool, false))
        {
            Log.Warning($"{ToPrettyString(args.User)} tried to start invalid surgery.");
            return;
        }

        var complete = IsStepComplete(ent, part, args.Step, surgery);
        args.Repeat = HasComp<SurgeryRepeatableStepComponent>(step) && !complete;
        var ev = new SurgeryStepEvent(args.User, ent, part, tool, surgery, step, complete);
        RaiseLocalEvent(step, ref ev);
        RaiseLocalEvent(args.User, ref ev);

        // consume the tool if it's something like using LV cable as stitches
        if (args.ToolUsed)
        {
            if (_stackQuery.TryComp(tool, out var stack))
                _stack.Use(tool, 1, stack);
            else
                PredictedQueueDel(tool);
        }

        RefreshUI(ent);
    }

    private void OnCloseIncisionValid(Entity<SurgeryCloseIncisionConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (!HasComp<IncisionOpenComponent>(args.Part) ||
            !HasComp<BleedersClampedComponent>(args.Part) ||
            !HasComp<SkinRetractedComponent>(args.Part) ||
            !HasComp<BodyPartReattachedComponent>(args.Part) ||
            !HasComp<InternalBleedersClampedComponent>(args.Part))
        {
            args.Cancelled = true;
        }
    }

    private void OnWoundedValid(Entity<SurgeryWoundedConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (!TryComp(args.Part, out WoundableComponent? partWoundable)
            || _wounds.GetWoundableSeverityPoint(
                args.Part,
                partWoundable,
                ent.Comp.DamageGroup,
                healable: true) <= 0)
            args.Cancelled = true;
    }

    private void OnBodyComponentConditionValid(Entity<SurgeryBodyComponentConditionComponent> ent, ref SurgeryValidEvent args)
    {
        var present = true;
        foreach (var reg in ent.Comp.Components.Values)
        {
            var compType = reg.Component.GetType();
            if (!HasComp(args.Body, compType))
                present = false;
        }

        if (ent.Comp.Inverse ? present : !present)
            args.Cancelled = true;
    }

    private void OnPartComponentConditionValid(Entity<SurgeryPartComponentConditionComponent> ent, ref SurgeryValidEvent args)
    {
        var present = true;
        foreach (var reg in ent.Comp.Components.Values)
        {
            var compType = reg.Component.GetType();
            if (!HasComp(args.Part, compType))
                present = false;
        }

        args.Cancelled |= present == ent.Comp.Inverse;
    }

    // This is literally a duplicate of the checks in OnToolCheck for SurgeryStepComponent.AddOrganOnAdd
    private void OnOrganOnAddConditionValid(Entity<SurgeryOrganOnAddConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (!TryComp<BodyPartComponent>(args.Part, out var part)
            || part.Body != args.Body)
        {
            args.Cancelled = true;
            return;
        }

        var organSlotIdToOrgan = _body.GetPartOrgans(args.Part, part).ToDictionary(o => o.Item2.SlotId, o => o.Item2);

        var allOnAddFound = true;
        var zeroOnAddFound = true;

        foreach (var (organSlotId, components) in ent.Comp.Components)
        {
            if (!organSlotIdToOrgan.TryGetValue(organSlotId, out var organ))
                continue;

            if (organ.OnAdd == null)
            {
                allOnAddFound = false;
                continue;
            }

            foreach (var key in components.Keys)
            {
                if (!organ.OnAdd.ContainsKey(key))
                    allOnAddFound = false;
                else
                    zeroOnAddFound = false;
            }
        }

        if (ent.Comp.Inverse ? allOnAddFound : zeroOnAddFound)
            args.Cancelled = true;
    }

    private void OnHasBodyConditionValid(Entity<SurgeryHasBodyConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (CompOrNull<BodyPartComponent>(args.Part)?.Body == null)
            args.Cancelled = true;
    }

    private void OnPartConditionValid(Entity<SurgeryPartConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (!TryComp<BodyPartComponent>(args.Part, out var part))
        {
            args.Cancelled = true;
            return;
        }

        var typeMatch = ent.Comp.Parts.Contains(part.PartType);
        var symmetryMatch = ent.Comp.Symmetry == null || part.Symmetry == ent.Comp.Symmetry;
        var valid = typeMatch && symmetryMatch;

        if (ent.Comp.Inverse ? valid : !valid)
            args.Cancelled = true;
    }

    private void OnOrganConditionValid(Entity<SurgeryOrganConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (!TryComp<BodyPartComponent>(args.Part, out var partComp)
            || partComp.Body != args.Body
            || ent.Comp.Organ == null)
        {
            args.Cancelled = true;
            return;
        }

        foreach (var reg in ent.Comp.Organ.Values)
        {
            if (_body.TryGetBodyPartOrgans(args.Part, reg.Component.GetType(), out var organs)
                && organs.Count > 0)
            {
                if (ent.Comp.Inverse
                    && (!ent.Comp.Reattaching
                    || ent.Comp.Reattaching
                    && !organs.Any(organ => HasComp<OrganReattachedComponent>(organ.Id))))
                    args.Cancelled = true;
                // Start of DeltaV Additions - Checks if any organ has the removable component set to true, hiding it from the surgery UI
                if (!organs.Any(organ => !TryComp<OrganComponent>(organ.Id, out var organComp)
                    || organComp.Removable))
                    args.Cancelled = true;
                // End of DeltaV Additions
            }
            else if (!ent.Comp.Inverse)
                args.Cancelled = true;
        }
    }

    private void OnBodyConditionValid(Entity<SurgeryBodyConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (_bodyQuery.CompOrNull(args.Body)?.Prototype is { } bodyId)
            args.Cancelled |= ent.Comp.Accepted.Contains(bodyId) == ent.Comp.Inverse;
    }

    private void OnOrganSlotConditionValid(Entity<SurgeryOrganSlotConditionComponent> ent, ref SurgeryValidEvent args)
    {
        args.Cancelled |= _body.CanInsertOrgan(args.Part, ent.Comp.OrganSlot) ^ !ent.Comp.Inverse;
    }

    private void OnPartRemovedConditionValid(Entity<SurgeryPartRemovedConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (!_body.CanAttachToSlot(args.Part, ent.Comp.Connection))
        {
            args.Cancelled = true;
            return;
        }

        var results = _body.GetBodyChildrenOfType(args.Body, ent.Comp.Part, symmetry: ent.Comp.Symmetry).ToList();
        if (results is not { } || !results.Any())
            return;

        if (!results.Any(part => HasComp<BodyPartReattachedComponent>(part.Id)))
            args.Cancelled = true;
    }

    private void OnPartPresentConditionValid(Entity<SurgeryPartPresentConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (args.Part == EntityUid.Invalid
            || !HasComp<BodyPartComponent>(args.Part))
            args.Cancelled = true;
    }

    private void OnTraumaPresentConditionValid(Entity<SurgeryTraumaPresentConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (args.Cancelled)
            return;

        // not inverted = cancel if no trauma present
        // inverted = cancel if trauma present
        if (_trauma.HasWoundableTrauma(args.Part, ent.Comp.TraumaType) == ent.Comp.Inverted)
            args.Cancelled = true;
    }

    private void OnBleedsPresentConditionValid(Entity<SurgeryBleedsPresentConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (!TryComp<WoundableComponent>(args.Part, out var woundable))
        {
            args.Cancelled = true;
            return;
        }

        if (ent.Comp.Inverted == woundable.Bleeds > 0
            && !HasComp<BleedersClampedComponent>(args.Part))
            args.Cancelled = true;
    }

    private void OnMarkingPresentValid(Entity<SurgeryMarkingConditionComponent> ent, ref SurgeryValidEvent args)
    {
        var markingCategory = MarkingCategoriesConversion.FromHumanoidVisualLayers(ent.Comp.MarkingCategory);

        var hasMarking = TryComp(args.Body, out HumanoidAppearanceComponent? bodyAppearance)
            && bodyAppearance.MarkingSet.Markings.TryGetValue(markingCategory, out var markingList)
            && markingList.Any(marking => marking.MarkingId.Contains(ent.Comp.MatchString));

        if ((!ent.Comp.Inverse && hasMarking) || (ent.Comp.Inverse && !hasMarking))
            args.Cancelled = true;
    }

    protected bool IsSurgeryValid(EntityUid body, EntityUid targetPart, EntProtoId surgery, EntProtoId stepId,
        EntityUid user, out Entity<SurgeryComponent> surgeryEnt, out EntityUid part, out EntityUid step)
    {
        surgeryEnt = default;
        part = default;
        step = default;

        if (!HasComp<SurgeryTargetComponent>(body) ||
            !IsLyingDown(body, user) ||
            GetSingleton(surgery) is not { } surgeryEntId ||
            !TryComp(surgeryEntId, out SurgeryComponent? surgeryComp) ||
            !surgeryComp.Steps.Contains(stepId) ||
            GetSingleton(stepId) is not { } stepEnt
            || !HasComp<BodyPartComponent>(targetPart)
            && !_bodyQuery.HasComp(targetPart))
            return false;


        var ev = new SurgeryValidEvent(body, targetPart);
        if (_timing.IsFirstTimePredicted)
        {
            RaiseLocalEvent(stepEnt, ref ev);
            if (!ev.Cancelled)
                RaiseLocalEvent(surgeryEntId, ref ev);
        }

        if (ev.Cancelled)
            return false;

        surgeryEnt = (surgeryEntId, surgeryComp);
        part = targetPart;
        step = stepEnt;
        return true;
    }

    public EntityUid? GetSingleton(EntProtoId surgeryOrStep)
    {
        if (!_prototypes.HasIndex(surgeryOrStep))
            return null;

        // This (for now) assumes that surgery entity data remains unchanged between client
        // and server
        // if it does not you get the bullet
        if (!_surgeries.TryGetValue(surgeryOrStep, out var ent) || TerminatingOrDeleted(ent))
        {
            ent = Spawn(surgeryOrStep, MapCoordinates.Nullspace);
            _surgeries[surgeryOrStep] = ent;
        }

        return ent;
    }

    /// <summary>
    /// Checks if someone is lying down (and is able to)
    /// Shows a popup if this is run on the user's client.
    /// </summary>
    public bool IsLyingDown(EntityUid entity, EntityUid user)
    {
        if (_standing.IsDown(entity))
            return true;

        // you can't otherwise operate on something with no buckle
        // just let people do surgery on goliaths and shit
        if (!TryComp<BuckleComponent>(entity, out var buckle))
            return true;

        if (TryComp<StrapComponent>(buckle.BuckledTo, out var strap))
        {
            var rotation = strap.Rotation;
            if (rotation.GetCardinalDir() is Direction.West or Direction.East)
                return true;
        }

        _popup.PopupClient(Loc.GetString("surgery-error-laying"), user, user);
        return false;
    }

    protected virtual void RefreshUI(EntityUid body)
    {
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<EntityPrototype>())
            return;

        LoadPrototypes();
    }

    private void LoadPrototypes()
    {
        // Cache is probably invalid so delete it
        foreach (var uid in _surgeries.Values)
        {
            Del(uid);
        }
        _surgeries.Clear();

        _allSurgeries.Clear();
        foreach (var entity in _prototypes.EnumeratePrototypes<EntityPrototype>())
            if (entity.HasComponent<SurgeryComponent>())
                _allSurgeries.Add(new EntProtoId(entity.ID));
    }
}
