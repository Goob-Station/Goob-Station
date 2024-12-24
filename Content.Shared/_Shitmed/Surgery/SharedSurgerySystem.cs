using System.Linq;
using Content.Shared._Shitmed.Medical.Surgery.Conditions;
using Content.Shared._Shitmed.Medical.Surgery.Effects.Complete;
using Content.Shared.Body.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Steps;
using Content.Shared._Shitmed.Medical.Surgery.Steps.Parts;
//using Content.Shared._RMC14.Xenonids.Parasite;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Body.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Prototypes;
using Content.Shared.Standing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Shitmed.Medical.Surgery;

public abstract partial class SharedSurgerySystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly RotateToFaceSystem _rotateToFace = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

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

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);

        SubscribeLocalEvent<SurgeryTargetComponent, SurgeryDoAfterEvent>(OnTargetDoAfter);
        SubscribeLocalEvent<SurgeryCloseIncisionConditionComponent, SurgeryValidEvent>(OnCloseIncisionValid);
        //SubscribeLocalEvent<SurgeryLarvaConditionComponent, SurgeryValidEvent>(OnLarvaValid);
        SubscribeLocalEvent<SurgeryHasBodyConditionComponent, SurgeryValidEvent>(OnHasBodyConditionValid);
        SubscribeLocalEvent<SurgeryPartConditionComponent, SurgeryValidEvent>(OnPartConditionValid);
        SubscribeLocalEvent<SurgeryOrganConditionComponent, SurgeryValidEvent>(OnOrganConditionValid);
        SubscribeLocalEvent<SurgeryWoundedConditionComponent, SurgeryValidEvent>(OnWoundedValid);
        SubscribeLocalEvent<SurgeryPartRemovedConditionComponent, SurgeryValidEvent>(OnPartRemovedConditionValid);
        SubscribeLocalEvent<SurgeryPartPresentConditionComponent, SurgeryValidEvent>(OnPartPresentConditionValid);
        SubscribeLocalEvent<SurgeryMarkingConditionComponent, SurgeryValidEvent>(OnMarkingPresentValid);
        SubscribeLocalEvent<SurgeryBodyComponentConditionComponent, SurgeryValidEvent>(OnBodyComponentConditionValid);
        SubscribeLocalEvent<SurgeryPartComponentConditionComponent, SurgeryValidEvent>(OnPartComponentConditionValid);
        SubscribeLocalEvent<SurgeryOrganOnAddConditionComponent, SurgeryValidEvent>(OnOrganOnAddConditionValid);
        //SubscribeLocalEvent<SurgeryRemoveLarvaComponent, SurgeryCompletedEvent>(OnRemoveLarva);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        InitializeSteps();

        LoadPrototypes();
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _surgeries.Clear();
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

        if (args.Handled
            || args.Target is not { } target
            || !IsSurgeryValid(ent, target, args.Surgery, args.Step, args.User, out var surgery, out var part, out var step)
            || !PreviousStepsComplete(ent, part, surgery, args.Step)
            || !CanPerformStep(args.User, ent, part, step, false))
        {
            Log.Warning($"{ToPrettyString(args.User)} tried to start invalid surgery.");
            return;
        }

        var complete = IsStepComplete(ent, part, args.Step, surgery);
        args.Repeat = HasComp<SurgeryRepeatableStepComponent>(step) && !complete;
        var ev = new SurgeryStepEvent(args.User, ent, part, GetTools(args.User), surgery, step, complete);
        RaiseLocalEvent(step, ref ev);
        RaiseLocalEvent(args.User, ref ev);
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
        if (!TryComp(args.Body, out DamageableComponent? damageable)
            || !TryComp(args.Part, out DamageableComponent? partDamageable)
            || damageable.TotalDamage <= 0
            && partDamageable.TotalDamage <= 0
            && !HasComp<IncisionOpenComponent>(args.Part))
            args.Cancelled = true;
    }

    /*private void OnLarvaValid(Entity<SurgeryLarvaConditionComponent> ent, ref SurgeryValidEvent args)
    {
        if (!TryComp(args.Body, out VictimInfectedComponent? infected))
            args.Cancelled = true;

        // The larva has fully developed and surgery is now impossible
        if (infected != null && infected.SpawnedLarva != null)
            args.Cancelled = true;
    }*/

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
        if (ent.Comp.Inverse ? present : !present)
            args.Cancelled = true;
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

        var typeMatch = part.PartType == ent.Comp.Part;
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
            }
            else if (!ent.Comp.Inverse)
                args.Cancelled = true;
        }
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

    private void OnMarkingPresentValid(Entity<SurgeryMarkingConditionComponent> ent, ref SurgeryValidEvent args)
    {
        var markingCategory = MarkingCategoriesConversion.FromHumanoidVisualLayers(ent.Comp.MarkingCategory);

        var hasMarking = TryComp(args.Body, out HumanoidAppearanceComponent? bodyAppearance)
            && bodyAppearance.MarkingSet.Markings.TryGetValue(markingCategory, out var markingList)
            && markingList.Any(marking => marking.MarkingId.Contains(ent.Comp.MatchString));

        if ((!ent.Comp.Inverse && hasMarking) || (ent.Comp.Inverse && !hasMarking))
            args.Cancelled = true;
    }

    /*private void OnRemoveLarva(Entity<SurgeryRemoveLarvaComponent> ent, ref SurgeryCompletedEvent args)
    {
        RemCompDeferred<VictimInfectedComponent>(ent);
    }*/

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
            && !HasComp<BodyComponent>(targetPart))
            return false;


        var ev = new SurgeryValidEvent(body, targetPart);
        if (_timing.IsFirstTimePredicted)
        {
            RaiseLocalEvent(stepEnt, ref ev);
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

    private List<EntityUid> GetTools(EntityUid surgeon)
    {
        return _hands.EnumerateHeld(surgeon).ToList();
    }

    public bool IsLyingDown(EntityUid entity, EntityUid user)
    {
        if (_standing.IsDown(entity))
            return true;

        if (TryComp(entity, out BuckleComponent? buckle) &&
            TryComp(buckle.BuckledTo, out StrapComponent? strap))
        {
            var rotation = strap.Rotation;
            if (rotation.GetCardinalDir() is Direction.West or Direction.East)
                return true;
        }

        _popup.PopupEntity(Loc.GetString("surgery-error-laying"), user, user);

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
