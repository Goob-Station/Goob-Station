    using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Audio;
using Content.Server.Cargo.Systems;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Server.Chemistry.EntitySystems;
using Content.Server.Construction;
using Content.Server.DoAfter;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Kitchen.Components;
using Content.Server.Nutrition;
using Content.Server.Nutrition.Components;
using Content.Server.Nyanotrasen.Kitchen.Components;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Construction;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Database;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.EntityEffects;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Nutrition;
using Content.Shared.Nyanotrasen.Kitchen;
using Content.Shared.Nyanotrasen.Kitchen.Components;
using Content.Shared.Nyanotrasen.Kitchen.UI;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Throwing;
using Content.Shared.UserInterface;
using Content.Shared.Whitelist;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Physics.Components;

namespace Content.Server.Nyanotrasen.Kitchen.EntitySystems;

public sealed partial class DeepFryerSystem : SharedDeepfryerSystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly IGameTiming _gameTimingSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly SolutionTransferSystem _solutionTransferSystem = default!;
    [Dependency] private readonly PuddleSystem _puddleSystem = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly AmbientSoundSystem _ambientSoundSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    private static readonly string CookingDamageType = "Heat";
    private static readonly float CookingDamageAmount = 10.0f;
    private static readonly float PvsWarningRange = 0.5f;
    private static readonly float ThrowMissChance = 0.25f;
    private static readonly int MaximumCrispiness = 2;
    private static readonly float BloodToProteinRatio = 0.1f;
    private static readonly string MobFlavorMeat = "meaty";

    private static readonly AudioParams
        AudioParamsInsertRemove = new(0.5f, 1f, 5f, 1.5f, 1f, false, 0f, 0.2f);

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("deepfryer");

        SubscribeLocalEvent<DeepFryerComponent, ComponentInit>(OnInitDeepFryer);
        SubscribeLocalEvent<DeepFryerComponent, PowerChangedEvent>(OnPowerChange);
        SubscribeLocalEvent<DeepFryerComponent, RefreshPartsEvent>(OnRefreshParts);
        SubscribeLocalEvent<DeepFryerComponent, MachineDeconstructedEvent>(OnDeconstruct);
        SubscribeLocalEvent<DeepFryerComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<DeepFryerComponent, ThrowHitByEvent>(OnThrowHitBy);
        SubscribeLocalEvent<DeepFryerComponent, SolutionChangedEvent>(OnSolutionChange);
        SubscribeLocalEvent<DeepFryerComponent, ContainerRelayMovementEntityEvent>(OnRelayMovement);
        SubscribeLocalEvent<DeepFryerComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<DeepFryerComponent, CanDropTargetEvent>(OnCanDragDropOn);
        SubscribeLocalEvent<DeepFryerComponent, DragDropTargetEvent>(OnDragDropOn);

        SubscribeLocalEvent<DeepFryerComponent, BeforeActivatableUIOpenEvent>(OnBeforeActivatableUIOpen);
        SubscribeLocalEvent<DeepFryerComponent, DeepFryerRemoveItemMessage>(OnRemoveItem);
        SubscribeLocalEvent<DeepFryerComponent, DeepFryerInsertItemMessage>(OnInsertItem);
        SubscribeLocalEvent<DeepFryerComponent, DeepFryerScoopVatMessage>(OnScoopVat);
        SubscribeLocalEvent<DeepFryerComponent, DeepFryerClearSlagMessage>(OnClearSlagStart);
        SubscribeLocalEvent<DeepFryerComponent, DeepFryerRemoveAllItemsMessage>(OnRemoveAllItems);
        SubscribeLocalEvent<DeepFryerComponent, ClearSlagDoAfterEvent>(OnClearSlag);

        SubscribeLocalEvent<DeepFriedComponent, ComponentInit>(OnInitDeepFried);
        SubscribeLocalEvent<DeepFriedComponent, ExaminedEvent>(OnExamineFried);
        SubscribeLocalEvent<DeepFriedComponent, PriceCalculationEvent>(OnPriceCalculation);
        SubscribeLocalEvent<DeepFriedComponent, FoodSlicedEvent>(OnSliceDeepFried);
    }

    private void UpdateUserInterface(EntityUid uid, DeepFryerComponent component)
    {
        var state = new DeepFryerBoundUserInterfaceState(
            GetOilLevel(uid, component),
            GetOilPurity(uid, component),
            component.FryingOilThreshold,
            EntityManager.GetNetEntityArray(component.Storage.ContainedEntities.ToArray()));

        _uiSystem.SetUiState(new Entity<UserInterfaceComponent?>(uid, null), DeepFryerUiKey.Key, state);
    }

    /// <summary>
    ///     Does the deep fryer have hot oil?
    /// </summary>
    /// <remarks>
    ///     This is mainly for audio.
    /// </remarks>
    private bool HasBubblingOil(EntityUid uid, DeepFryerComponent component)
    {
        return _powerReceiverSystem.IsPowered(uid) && GetOilVolume(uid, component) > FixedPoint2.Zero;
    }

    /// <summary>
    ///     Returns how much total oil is in the vat.
    /// </summary>
    public FixedPoint2 GetOilVolume(EntityUid uid, DeepFryerComponent component)
    {
        var oilVolume = FixedPoint2.Zero;

        foreach (var reagent in component.Solution)
        {
            if (component.FryingOils.Contains(reagent.Reagent.ToString()))
                oilVolume += reagent.Quantity;
        }

        return oilVolume;
    }

    private void OnDragDropOn(EntityUid uid, DeepFryerComponent component, ref DragDropTargetEvent args)
    {
        _containerSystem.Insert(args.Dragged, component.Storage);
        args.Handled = true;
    }

    /// <summary>
    ///     Returns how much total waste is in the vat.
    /// </summary>
    public FixedPoint2 GetWasteVolume(EntityUid uid, DeepFryerComponent component)
    {
        var wasteVolume = FixedPoint2.Zero;

        foreach (var reagent in component.WasteReagents)
        {
            wasteVolume += component.Solution.GetReagentQuantity(reagent.Reagent);
        }

        return wasteVolume;
    }

    /// <summary>
    ///     Returns a percentage of how much of the total solution is usable oil.
    /// </summary>
    public FixedPoint2 GetOilPurity(EntityUid uid, DeepFryerComponent component)
    {
        if (component.Solution.Volume == 0) return 0;
        return GetOilVolume(uid, component) / component.Solution.Volume;
    }

    /// <summary>
    ///     Returns a percentage of how much of the total volume is usable oil.
    /// </summary>
    public FixedPoint2 GetOilLevel(EntityUid uid, DeepFryerComponent component)
    {
        return GetOilVolume(uid, component) / component.Solution.MaxVolume;
    }

    /// <summary>
    ///     This takes care of anything that would happen to an item with or
    ///     without enough oil.
    /// </summary>
    private void CookItem(EntityUid uid, DeepFryerComponent component, EntityUid item)
    {
        if (TryComp<TemperatureComponent>(item, out var tempComp))
        {
            // Push the temperature towards what it should be but no higher.
            var delta = (component.PoweredTemperature - tempComp.CurrentTemperature) * _temperature.GetHeatCapacity(item, tempComp);

            if (delta > 0f)
                _temperature.ChangeHeat(item, delta, false, tempComp);
        }

        if (TryComp<SolutionContainerManagerComponent>(item, out var solutions) && solutions.Solutions != null)
        {
            foreach (var (_, solution) in solutions.Solutions)
            {
                if(_solutionContainerSystem.TryGetSolution(item, solution.Name, out var solutionRef))
                    _solutionContainerSystem.SetTemperature(solutionRef!.Value, component.PoweredTemperature);
            }
        }

        // Damage non-food items and mobs.
        if ((!HasComp<FoodComponent>(item) || HasComp<MobStateComponent>(item)) &&
            TryComp<DamageableComponent>(item, out var damageableComponent))
        {
            var damage = new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>(CookingDamageType),
                CookingDamageAmount);

            var result = _damageableSystem.TryChangeDamage(item, damage, origin: uid);
            if (result?.GetTotal() > FixedPoint2.Zero)
            {
                // TODO: Smoke, waste, sound, or some indication.
            }
        }
    }

    /// <summary>
    ///     Destroy a food item and replace it with a charred mess.
    /// </summary>
    private void BurnItem(EntityUid uid, DeepFryerComponent component, EntityUid item)
    {
        if (HasComp<FoodComponent>(item) &&
            !HasComp<MobStateComponent>(item) &&
            MetaData(item).EntityPrototype?.ID != component.CharredPrototype)
        {
            var charred = Spawn(component.CharredPrototype, Transform(uid).Coordinates);
            _containerSystem.Insert(charred, component.Storage);
            Del(item);
        }
    }

    private void UpdateDeepFriedName(EntityUid uid, DeepFriedComponent component)
    {
        if (component.OriginalName == null)
            return;

        switch (component.Crispiness)
        {
            case 0:
                // Already handled at OnInitDeepFried.
                break;
            case 1:
                _metaDataSystem.SetEntityName(uid, Loc.GetString("deep-fried-crispy-item",
                    ("entity", component.OriginalName)));
                break;
            default:
                _metaDataSystem.SetEntityName(uid, Loc.GetString("deep-fried-burned-item",
                    ("entity", component.OriginalName)));
                break;
        }
    }

    /// <summary>
    ///     Try to deep fry a single item, which can
    ///     - be cancelled by other systems, or
    ///     - fail due to the blacklist, or
    ///     - give it a crispy shader, and possibly also
    ///     - turn it into food.
    /// </summary>
    private void DeepFry(EntityUid uid, DeepFryerComponent component, EntityUid item)
    {
        if (MetaData(item).EntityPrototype?.ID == component.CharredPrototype)
            return;

        // This item has already been deep-fried, and now it's progressing
        // into another stage.
        if (TryComp<DeepFriedComponent>(item, out var deepFriedComponent))
        {
            // TODO: Smoke, waste, sound, or some indication.

            deepFriedComponent.Crispiness += 1;

            if (deepFriedComponent.Crispiness > MaximumCrispiness)
            {
                BurnItem(uid, component, item);
                return;
            }

            UpdateDeepFriedName(item, deepFriedComponent);
            return;
        }

        // Allow entity systems to conditionally forbid an attempt at deep-frying.
        var attemptEvent = new DeepFryAttemptEvent(uid);
        RaiseLocalEvent(item, attemptEvent);

        if (attemptEvent.Cancelled)
            return;

        // The attempt event is allowed to go first before the blacklist check,
        // just in case the attempt is relevant to any system in the future.
        //
        // The blacklist overrides all.
        if (component.Blacklist != null && _whitelistSystem.IsWhitelistPass(component.Blacklist, item))
        {
            _popupSystem.PopupEntity(
                Loc.GetString("deep-fryer-blacklist-item-failed",
                    ("item", item), ("deepFryer", uid)),
                uid,
                Filter.Pvs(uid, PvsWarningRange),
                true);
            return;
        }

        var beingEvent = new BeingDeepFriedEvent(uid, item);
        RaiseLocalEvent(item, beingEvent);

        // It's important to check for the MobStateComponent so we know
        // it's actually a mob, because functions like
        // MobStateSystem.IsAlive will return false if the entity lacks the
        // component.
        if (TryComp<MobStateComponent>(item, out var mobStateComponent))
        {
            if (!TryMakeMobIntoFood(item, mobStateComponent))
                return;
        }

        MakeCrispy(item);

        var oilToUse = 0;

        if (HasComp<ItemComponent>(item)) {
            var itemComponent = Comp<ItemComponent>(item);

            oilToUse = (int) (itemComponent.Size.Id switch
            {
                "Tiny" => 1,
                "Small" => 5,
                "Medium" => 10,
                "Large" => 15,
                "Huge" => 30,
                "Ginormous" => 50,
                _ => 10
            } * component.SolutionSizeCoefficient);
        } else {
            oilToUse = (int) (TryComp<PhysicsComponent>(item, out var physicsComponent) ? physicsComponent.Mass : 10);
        }

        // Determine how much solution to spend on this item.
        var solutionQuantity = FixedPoint2.Min(
            component.Solution.Volume,
            oilToUse
        );

        if (component.Whitelist != null && _whitelistSystem.IsWhitelistPass(component.Whitelist, item) ||
            beingEvent.TurnIntoFood)
            MakeEdible(uid, component, item, solutionQuantity);
        else
            component.Solution.RemoveSolution(solutionQuantity);

        component.WasteToAdd += solutionQuantity;
    }

    private void OnInitDeepFryer(EntityUid uid, DeepFryerComponent component, ComponentInit args)
    {
        component.Storage =
            _containerSystem.EnsureContainer<Container>(uid, component.StorageName, out var containerExisted);

        if (!containerExisted)
            _sawmill.Warning(
                $"{ToPrettyString(uid)} did not have a {component.StorageName} container. It has been created.");

        component.Solution =
            _solutionContainerSystem.EnsureSolution(uid, component.SolutionName, out var solutionExisted);

        if (!solutionExisted)
            _sawmill.Warning(
                $"{ToPrettyString(uid)} did not have a {component.SolutionName} solution container. It has been created.");
        foreach (var reagent in component.Solution.Contents.ToArray())
        {
            //JJ Comment - not sure this works. Need to check if Reagent.ToString is correct.
            _prototypeManager.TryIndex<ReagentPrototype>(reagent.Reagent.ToString(), out var proto);
            var effectsArgs = new EntityEffectReagentArgs(uid,
                EntityManager,
                null,
                component.Solution,
                reagent.Quantity,
                proto!,
                null,
                1f);
            foreach (var effect in component.UnsafeOilVolumeEffects)
            {
                if (!effect.ShouldApply(effectsArgs, _random))
                    continue;
                effect.Effect(effectsArgs);
            }
        }
    }

    /// <summary>
    ///     Make sure the UI and interval tracker are updated anytime something
    ///     is inserted into one of the baskets.
    /// </summary>
    /// <remarks>
    ///     This is used instead of EntInsertedIntoContainerMessage so charred
    ///     items can be inserted into the deep fryer without triggering this
    ///     event.
    /// </remarks>
    private void AfterInsert(EntityUid uid, DeepFryerComponent component, EntityUid item)
    {
        if (HasBubblingOil(uid, component))
            _audioSystem.PlayPvs(component.SoundInsertItem, uid, AudioParamsInsertRemove);

        UpdateNextFryTime(uid, component);
        UpdateUserInterface(uid, component);
    }

    private void OnPowerChange(EntityUid uid, DeepFryerComponent component, ref PowerChangedEvent args)
    {
        _appearanceSystem.SetData(uid, DeepFryerVisuals.Bubbling, args.Powered);
        UpdateNextFryTime(uid, component);
        UpdateAmbientSound(uid, component);
    }

    private void OnDeconstruct(EntityUid uid, DeepFryerComponent component, MachineDeconstructedEvent args)
    {
        // The EmptyOnMachineDeconstruct component handles the entity container for us.
        _puddleSystem.TrySpillAt(uid, component.Solution, out var _);
    }

    private void OnDestruction(EntityUid uid, DeepFryerComponent component, DestructionEventArgs args)
    {
        _containerSystem.EmptyContainer(component.Storage, true);
    }

    private void OnRefreshParts(EntityUid uid, DeepFryerComponent component, RefreshPartsEvent args)
    {
        var ratingStorage = args.PartRatings[component.MachinePartStorageMax];

        component.StorageMaxEntities = component.BaseStorageMaxEntities +
                                       (int) (component.StoragePerPartRating * (ratingStorage - 1));
    }

    /// <summary>
    ///     Allow thrown items to land in a basket.
    /// </summary>
    private void OnThrowHitBy(EntityUid uid, DeepFryerComponent component, ThrowHitByEvent args)
    {
        if (args.Handled)
            return;

        // Chefs never miss this. :)
        var missChance = HasComp<ProfessionalChefComponent>(args.User) ? 0f : ThrowMissChance;

        if (!CanInsertItem(uid, component, args.Thrown) ||
            _random.Prob(missChance) ||
            !_containerSystem.Insert(args.Thrown, component.Storage))
        {
            _popupSystem.PopupEntity(
                Loc.GetString("deep-fryer-thrown-missed"),
                uid);

            if (args.User != null)
            {
                _adminLogManager.Add(LogType.Action, LogImpact.Low,
                    $"{ToPrettyString(args.User.Value)} threw {ToPrettyString(args.Thrown)} at {ToPrettyString(uid)}, and it missed.");
            }

            return;
        }

        if (GetOilVolume(uid, component) < component.SafeOilVolume)
        {
            _popupSystem.PopupEntity(
                Loc.GetString("deep-fryer-thrown-hit-oil-low"),
                uid);
        }
        else
        {
            _popupSystem.PopupEntity(
                Loc.GetString("deep-fryer-thrown-hit-oil"),
                uid);
        }

        if (args.User != null)
        {
            _adminLogManager.Add(LogType.Action, LogImpact.Low,
                $"{ToPrettyString(args.User.Value)} threw {ToPrettyString(args.Thrown)} at {ToPrettyString(uid)}, and it landed inside.");
        }

        AfterInsert(uid, component, args.Thrown);

        args.Handled = true;
    }

    private void OnSolutionChange(EntityUid uid, DeepFryerComponent component, SolutionChangedEvent args)
    {
        UpdateUserInterface(uid, component);
        UpdateAmbientSound(uid, component);
    }

    private void OnRelayMovement(EntityUid uid, DeepFryerComponent component,
        ref ContainerRelayMovementEntityEvent args)
    {
        if (!_containerSystem.Remove(args.Entity, component.Storage, destination: Transform(uid).Coordinates))
            return;

        _popupSystem.PopupEntity(
            Loc.GetString("deep-fryer-entity-escape",
                ("victim", Identity.Entity(args.Entity, EntityManager)),
                ("deepFryer", uid)),
            uid,
            PopupType.SmallCaution);
    }

    private void OnBeforeActivatableUIOpen(EntityUid uid, DeepFryerComponent component,
        BeforeActivatableUIOpenEvent args)
    {
        UpdateUserInterface(uid, component);
    }

    private void OnRemoveItem(EntityUid uid, DeepFryerComponent component, DeepFryerRemoveItemMessage args)
    {
        var removedItem = EntityManager.GetEntity(args.Item);
        if (removedItem.Valid)
        {
            //JJ Comment - This line should be unnecessary. Some issue is keeping the UI from updating when converting straight to a Burned Mess while the UI is still open. To replicate, put a Raw Meat in the fryer with no oil in it. Wait until it sputters with no effect. It should transform to Burned Mess, but doesn't.
            if (!_containerSystem.Remove(removedItem, component.Storage))
                return;

            var user = EntityManager.GetEntity(args.Entity);

            _handsSystem.TryPickupAnyHand(user, removedItem);

            _adminLogManager.Add(LogType.Action, LogImpact.Low,
                $"{ToPrettyString(user)} took {ToPrettyString(args.Item)} out of {ToPrettyString(uid)}.");

            _audioSystem.PlayPvs(component.SoundRemoveItem, uid, AudioParamsInsertRemove);

            UpdateUserInterface(component.Owner, component);
        }
    }

    /// <summary>
    ///     This is a helper function for ScoopVat and ClearSlag.
    /// </summary>
    private bool TryGetActiveHandSolutionContainer(
        EntityUid fryer,
        EntityUid user,
        [NotNullWhen(true)] out EntityUid? heldItem,
        [NotNullWhen(true)] out Entity<SolutionComponent>? solution,
        out FixedPoint2 transferAmount)
    {
        heldItem = null;
        solution = null;
        transferAmount = FixedPoint2.Zero;

        if (!TryComp<HandsComponent>(user, out var handsComponent))
            return false;

        heldItem = handsComponent.ActiveHandEntity;

        if (heldItem == null ||
            !TryComp<SolutionTransferComponent>(heldItem, out var solutionTransferComponent) ||
            !_solutionContainerSystem.TryGetRefillableSolution(heldItem.Value, out var solEnt, out var _) ||
            !solutionTransferComponent.CanReceive)
        {
            _popupSystem.PopupEntity(
                Loc.GetString("deep-fryer-need-liquid-container-in-hand"),
                fryer,
                user);

            return false;
        }

        solution = solEnt;
        transferAmount = solutionTransferComponent.TransferAmount;

        return true;
    }

    private void OnScoopVat(EntityUid uid, DeepFryerComponent component, DeepFryerScoopVatMessage args)
    {
        var user = EntityManager.GetEntity(args.Entity);

        if (!TryGetActiveHandSolutionContainer(uid, user, out var heldItem, out var heldSolution,
                out var transferAmount))
            return;

        if (!_solutionContainerSystem.TryGetSolution(component.Owner, component.Solution.Name, out var solution))
            return;

        _solutionTransferSystem.Transfer(user,
            uid,
            solution.Value,
            heldItem.Value,
            heldSolution.Value,
            transferAmount);

        // UI update is not necessary here, because the solution change event handles it.
    }

    private void OnClearSlagStart(EntityUid uid, DeepFryerComponent component, DeepFryerClearSlagMessage args)
    {
        var user = EntityManager.GetEntity(args.Entity);

        if (!TryGetActiveHandSolutionContainer(uid, user, out var heldItem, out var heldSolution,
                out var transferAmount))
            return;

        var wasteVolume = GetWasteVolume(uid, component);
        if (wasteVolume == FixedPoint2.Zero)
        {
            _popupSystem.PopupEntity(
                Loc.GetString("deep-fryer-oil-no-slag"),
                uid,
                user);

            return;
        }

        var delay = Math.Clamp((float) wasteVolume * 0.1f, 1f, 5f);

        var ev = new ClearSlagDoAfterEvent(heldSolution.Value.Comp.Solution, transferAmount);

        //JJ Comment - not sure I have DoAfterArgs configured correctly.
        var doAfterArgs = new DoAfterArgs(EntityManager, user, delay, ev, uid, uid, heldItem)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            MovementThreshold = 0.25f,
            NeedHand = true
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnRemoveAllItems(EntityUid uid, DeepFryerComponent component, DeepFryerRemoveAllItemsMessage args)
    {
        if (component.Storage.ContainedEntities.Count == 0)
            return;

        _containerSystem.EmptyContainer(component.Storage);

        var user = EntityManager.GetEntity(args.Entity);

        _adminLogManager.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(user)} removed all items from {ToPrettyString(uid)}.");

        _audioSystem.PlayPvs(component.SoundRemoveItem, uid, AudioParamsInsertRemove);

        UpdateUserInterface(component.Owner, component);
    }

    private void OnClearSlag(EntityUid uid, DeepFryerComponent component, ClearSlagDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Used == null)
            return;

        FixedPoint2 reagentCount = component.WasteReagents.Count();

        var removingSolution = new Solution();
        foreach (var reagent in component.WasteReagents)
        {
            var removed = component.Solution.RemoveReagent(reagent.Reagent.ToString(), args.Amount / reagentCount);
            removingSolution.AddReagent(reagent.Reagent.ToString(), removed);
        }

        if (!_solutionContainerSystem.TryGetSolution(uid, component.SolutionName, out var solution))
            return;

        if (!_solutionContainerSystem.TryGetSolution(args.Used!.Value, args.Solution.Name, out var targetSolution))
            return;

        _solutionContainerSystem.UpdateChemicals(solution.Value);
        _solutionContainerSystem.TryMixAndOverflow(targetSolution.Value, removingSolution,
            args.Solution.MaxVolume, out var _);
    }

    private void OnInitDeepFried(EntityUid uid, DeepFriedComponent component, ComponentInit args)
    {
        var meta = MetaData(uid);
        component.OriginalName = meta.EntityName;
        _metaDataSystem.SetEntityName(uid, Loc.GetString("deep-fried-crispy-item", ("entity", meta.EntityName)));
    }

    private void OnExamineFried(EntityUid uid, DeepFriedComponent component, ExaminedEvent args)
    {
        switch (component.Crispiness)
        {
            case 0:
                args.PushMarkup(Loc.GetString("deep-fried-crispy-item-examine"));
                break;
            case 1:
                args.PushMarkup(Loc.GetString("deep-fried-fried-item-examine"));
                break;
            default:
                args.PushMarkup(Loc.GetString("deep-fried-burned-item-examine"));
                break;
        }
    }

    private void OnPriceCalculation(EntityUid uid, DeepFriedComponent component, ref PriceCalculationEvent args)
    {
        args.Price *= component.PriceCoefficient;
    }

    private void OnSliceDeepFried(EntityUid uid, DeepFriedComponent component, FoodSlicedEvent args)
    {
        MakeCrispy(args.Slice);

        // Copy relevant values to the slice.
        var sourceDeepFriedComponent = Comp<DeepFriedComponent>(args.Food);
        var sliceDeepFriedComponent = Comp<DeepFriedComponent>(args.Slice);

        sliceDeepFriedComponent.Crispiness = sourceDeepFriedComponent.Crispiness;
        sliceDeepFriedComponent.PriceCoefficient = sourceDeepFriedComponent.PriceCoefficient;

        UpdateDeepFriedName(args.Slice, sliceDeepFriedComponent);

        // TODO: Flavor profiles aren't copied to the slices. This should
        // probably be handled on upstream, but for now let's assume the
        // oil of the deep fryer is overpowering enough for this small
        // hack. This is likely the only place where it would be useful.
        if (TryComp<FlavorProfileComponent>(args.Food, out var sourceFlavorProfileComponent) &&
            TryComp<FlavorProfileComponent>(args.Slice, out var sliceFlavorProfileComponent))
        {
            sliceFlavorProfileComponent.Flavors.UnionWith(sourceFlavorProfileComponent.Flavors);
            sliceFlavorProfileComponent.IgnoreReagents.UnionWith(sourceFlavorProfileComponent.IgnoreReagents);
        }
    }
}

public sealed class DeepFryAttemptEvent : CancellableEntityEventArgs
{
    public EntityUid DeepFryer { get; }

    public DeepFryAttemptEvent(EntityUid deepFryer)
    {
        DeepFryer = deepFryer;
    }
}

public sealed class BeingDeepFriedEvent : EntityEventArgs
{
    public EntityUid DeepFryer { get; }
    public EntityUid Item { get; }
    public bool TurnIntoFood { get; set; }

    public BeingDeepFriedEvent(EntityUid deepFryer, EntityUid item)
    {
        DeepFryer = deepFryer;
        Item = item;
    }
}
