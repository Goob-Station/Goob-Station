using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Content.Shared._White.Xenomorphs.Infection;
using Content.Shared.Body.Components; // Goobstation start
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._White.Xenomorphs.FaceHugger;
using Content.Shared.Mobs.Components;
using Content.Shared.Throwing;
using Content.Shared.Atmos.Components;
using Content.Server.Nutrition.EntitySystems;
using Content.Shared.Nutrition.Components; // Goobstation end

namespace Content.Server._White.Xenomorphs.FaceHugger;

public sealed class FaceHuggerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReactiveSystem _reactiveSystem = default!; // Goobstation
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!; // Goobstation
    [Dependency] private readonly SharedTransformSystem _transform = default!; // Goobstation

    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FaceHuggerComponent, StartCollideEvent>(OnCollideEvent);
        SubscribeLocalEvent<FaceHuggerComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<FaceHuggerComponent, GotEquippedHandEvent>(OnPickedUp);
        SubscribeLocalEvent<FaceHuggerComponent, StepTriggeredOffEvent>(OnStepTriggered);
        SubscribeLocalEvent<FaceHuggerComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<FaceHuggerComponent, BeingUnequippedAttemptEvent>(OnBeingUnequippedAttempt);

        // Goobstation - Throwing behavior
        SubscribeLocalEvent<ThrowableFacehuggerComponent, ThrownEvent>(OnThrown);
        SubscribeLocalEvent<ThrowableFacehuggerComponent, ThrowDoHitEvent>(OnThrowDoHit);
    }

    private void OnCollideEvent(EntityUid uid, FaceHuggerComponent component, StartCollideEvent args)
        => TryEquipFaceHugger(uid, args.OtherEntity, component);

    private void OnMeleeHit(EntityUid uid, FaceHuggerComponent component, MeleeHitEvent args)
    {
        if (args.HitEntities.FirstOrNull() is not { } target)
            return;

        TryEquipFaceHugger(uid, target, component);
    }

    private void OnPickedUp(EntityUid uid, FaceHuggerComponent component, GotEquippedHandEvent args)
        => TryEquipFaceHugger(uid, args.User, component);

    private void OnStepTriggered(EntityUid uid, FaceHuggerComponent component, ref StepTriggeredOffEvent args)
    {
        if (component.Active)
            TryEquipFaceHugger(uid, args.Tripper, component);
    }

    private void OnGotEquipped(EntityUid uid, FaceHuggerComponent component, GotEquippedEvent args)
    {
        if (args.Slot != component.Slot
            || _mobState.IsDead(uid)
            || _entityWhitelist.IsBlacklistPass(component.Blacklist, args.Equipee))
            return;
        _popup.PopupEntity(Loc.GetString("xenomorphs-face-hugger-equip", ("equipment", uid)), uid, args.Equipee);
        _popup.PopupEntity(
            Loc.GetString("xenomorphs-face-hugger-equip-other",
                ("equipment", uid),
                ("target", Identity.Entity(args.Equipee, EntityManager))),
            uid,
            Filter.PvsExcept(args.Equipee),
            true);

        _stun.TryKnockdown(args.Equipee, component.KnockdownTime, true);

        if (component.InfectionPrototype.HasValue)
            EnsureComp<XenomorphPreventSuicideComponent>(args.Equipee); //Prevent suicide for infected

        if (!component.InfectionPrototype.HasValue)
            return;

        component.InfectIn = _timing.CurTime + _random.Next(component.MinInfectTime, component.MaxInfectTime);
    }

    private void OnBeingUnequippedAttempt(EntityUid uid,
        FaceHuggerComponent component,
        BeingUnequippedAttemptEvent args)
    {
        if (component.Slot != args.Slot || args.Unequipee != args.UnEquipTarget ||
            !component.InfectionPrototype.HasValue || _mobState.IsDead(uid))
            return;

        _popup.PopupEntity(
            Loc.GetString("xenomorphs-face-hugger-unequip", ("equipment", Identity.Entity(uid, EntityManager))),
            uid,
            args.Unequipee);
        args.Cancel();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<FaceHuggerComponent>();
        while (query.MoveNext(out var uid, out var faceHugger))
        {
            if (!faceHugger.Active && time > faceHugger.RestIn)
                faceHugger.Active = true;

            if (faceHugger.InfectIn != TimeSpan.Zero && time > faceHugger.InfectIn)
            {
                faceHugger.InfectIn = TimeSpan.Zero;
                Infect(uid, faceHugger);
            }

            // Handle continuous chemical injection when equipped
            // Goobstation
            if (TryComp<ClothingComponent>(uid, out var clothing) && clothing.InSlot != null)
            {
                // Initialize NextInjectionTime if it's zero
                if (faceHugger.NextInjectionTime == TimeSpan.Zero)
                {
                    faceHugger.NextInjectionTime = time + faceHugger.InitialInjectionDelay;
                    continue;
                }

                if (time >= faceHugger.NextInjectionTime)
                {
                    // Get the entity that has this item equipped
                    if (_container.TryGetContainingContainer(uid, out var container) && container.Owner != uid)
                    {
                        InjectChemicals(uid, faceHugger, container.Owner);
                        // Set the next injection time based on the current time plus interval
                        faceHugger.NextInjectionTime = time + faceHugger.InjectionInterval;
                    }
                }
            }
            // Goobstaion end

            // Check for nearby entities to latch onto
            if (faceHugger.Active && clothing?.InSlot == null)
            {
                foreach (var entity in _entityLookup.GetEntitiesInRange<InventoryComponent>(Transform(uid).Coordinates,
                             1.5f))
                {
                    if (TryEquipFaceHugger(uid, entity, faceHugger))
                        break;
                }
            }
        }
    }

    private void Infect(EntityUid uid, FaceHuggerComponent component)
    {
        if (!component.InfectionPrototype.HasValue
            || !TryComp<ClothingComponent>(uid, out var clothing)
            || clothing.InSlot != component.Slot
            || !_container.TryGetContainingContainer((uid, null, null), out var target))
            return;

        var bodyPart = _body.GetBodyChildrenOfType(target.Owner,
                component.InfectionBodyPart.Type,
                symmetry: component.InfectionBodyPart.Symmetry)
            .FirstOrNull();
        if (!bodyPart.HasValue)
            return;

        var organ = Spawn(component.InfectionPrototype);
        _body.TryCreateOrganSlot(bodyPart.Value.Id, component.InfectionSlotId, out _, bodyPart.Value.Component);

        if (!_body.InsertOrgan(bodyPart.Value.Id, organ, component.InfectionSlotId, bodyPart.Value.Component))
        {
            QueueDel(organ);
            return;
        }

        _damageable.TryChangeDamage(uid, component.DamageOnInfect, true);
    }

    public bool TryEquipFaceHugger(EntityUid uid, EntityUid target, FaceHuggerComponent component)
    {
        if (!component.Active || _mobState.IsDead(uid) || _entityWhitelist.IsBlacklistPass(component.Blacklist, target))
            return false;

        // Check for any blocking masks or equipment
        if (CheckAndHandleMask(target, out var blocker))
        {
            // If blocked by a breathable mask, deal damage and schedule a retry
            if (blocker.HasValue && TryComp<BreathToolComponent>(blocker, out _))
            {
                // Deal damage to the target
                _damageable.TryChangeDamage(target, component.MaskBlockDamage);

                // Play the mask block sound
                _audio.PlayPvs(component.MaskBlockSound, uid);

                // Show popup messages
                _popup.PopupEntity(
                    Loc.GetString("xenomorphs-face-hugger-mask-blocked",
                        ("mask", blocker.Value),
                        ("facehugger", uid)),
                    target, target);

                _popup.PopupEntity(
                    Loc.GetString("xenomorphs-face-hugger-mask-blocked-other",
                        ("facehugger", uid),
                        ("target", target),
                        ("mask", blocker.Value)),
                    target, Filter.PvsExcept(target), true);

                // Schedule a retry after the delay
                component.RestIn = _timing.CurTime + component.AttachAttemptDelay;
                component.Active = false;

                // Drop the facehugger near you
                _transform.SetCoordinates(uid, Transform(target).Coordinates.Offset(_random.NextVector2(0.5f)));

                return false;
            }

            // Original behavior for other blockers
            _audio.PlayPvs(component.SoundOnImpact, uid);
            _damageable.TryChangeDamage(uid, component.DamageOnImpact);
            _popup.PopupEntity(
                Loc.GetString("xenomorphs-face-hugger-try-equip",
                    ("equipment", uid),
                    ("equipmentBlocker", blocker!.Value)),
                uid);

            _popup.PopupEntity(
                Loc.GetString("xenomorphs-face-hugger-try-equip-other",
                    ("equipment", uid),
                    ("equipmentBlocker", blocker.Value),
                    ("target", Identity.Entity(target, EntityManager))),
                uid, Filter.PvsExcept(target), true);

            return false;
        }

        // Set the rest time and deactivate
        var restTime = _random.Next(component.MinRestTime, component.MaxRestTime);
        component.RestIn = _timing.CurTime + restTime;
        component.Active = false;

        // Try to equip the facehugger
        return _inventory.TryEquip(target, uid, component.Slot, true, true);
    } // Gooobstation end

    #region Injection Code
    /// <summary>
    /// Checks if the facehugger can inject chemicals into the target
    /// Goobstation
    /// </summary>
    public bool CanInject(EntityUid uid, FaceHuggerComponent component, EntityUid target)
    {
        // Check if facehugger is properly equipped
        if (!TryComp<ClothingComponent>(uid, out var clothingComp) || clothingComp.InSlot == null)
        {
            if (!component.Active)
                return false;
            return true;
        }

        // Check if target already has the sleep chemical
        if (TryComp<BloodstreamComponent>(target, out var bloodstream) &&
            _solutions.ResolveSolution(target, bloodstream.ChemicalSolutionName, ref bloodstream.ChemicalSolution, out var chemSolution) &&
            chemSolution.TryGetReagentQuantity(new ReagentId(component.SleepChem, null), out var quantity) &&
            quantity > FixedPoint2.New(component.MinChemicalThreshold))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Creates a solution with the sleep chemical
    /// </summary>
    public Solution CreateSleepChemicalSolution(FaceHuggerComponent component, float amount)
    {
        var solution = new Solution();
        solution.AddReagent(component.SleepChem, amount);
        return solution;
    }

    /// <summary>
    /// Attempts to inject the solution into the target's bloodstream
    /// </summary>
    public bool TryInjectIntoBloodstream(EntityUid target, Solution solution, string chemName, float chemAmount)
    {
        if (!TryComp<BloodstreamComponent>(target, out var bloodstream))
            return false;

        if (!_solutions.TryGetSolution(target, bloodstream.ChemicalSolutionName, out var chemSolution, out _))
            return false;

        if (!_solutions.TryAddSolution(chemSolution.Value, solution))
            return false;

        _reactiveSystem.DoEntityReaction(target, solution, ReactionMethod.Injection);
        return true;
    }

    /// <summary>
    /// Main method to handle chemical injection
    /// </summary>
    public void InjectChemicals(EntityUid uid, FaceHuggerComponent component, EntityUid target)
    {
        if (!CanInject(uid, component, target))
            return;

        var sleepChem = CreateSleepChemicalSolution(component, component.SleepChemAmount);
        TryInjectIntoBloodstream(target, sleepChem, component.SleepChem, component.SleepChemAmount);
    }
    #endregion

    #region Handle Face Masks
    /// <summary>
    /// Checks if the target has a breathable mask or any other blocking equipment.
    /// Returns true if there's a blocker, false otherwise.
    /// Goobstation
    /// </summary>
    private bool CheckAndHandleMask(EntityUid target, out EntityUid? blocker)
    {
        blocker = null;

        // Check for breathable mask
        if (_inventory.TryGetSlotEntity(target, "mask", out var maskUid))
        {
            // If the mask is a breath tool (gas mask) and is functional, block the facehugger
            if (TryComp<IngestionBlockerComponent>(maskUid, out var ingestionBlocker) && ingestionBlocker.BlockSmokeIngestion)
            {
                blocker = maskUid;
                return true;
            }
            // If it's just a regular mask, remove it
            _inventory.TryUnequip(target, "mask", true);
        }

        return false;
    }
    #endregion

    #region Throwing Behavior

    /// <summary>
    /// Handles the start of a facehugger throw.
    /// Marks the facehugger as being in flight to track its state.
    /// Goobstation
    /// </summary>
    private void OnThrown(EntityUid uid, ThrowableFacehuggerComponent component, ThrownEvent args)
    {
        // Mark the facehugger as flying to track its airborne state
        component.IsFlying = true;

        // Make sure the facehugger is active when thrown
        if (TryComp<FaceHuggerComponent>(uid, out var faceHugger))
            faceHugger.Active = true;
    }

    /// <summary>
    /// Handles the facehugger's collision with a target after being thrown.
    /// Attempts to attach to a valid target if conditions are met.
    /// </summary>
    private void OnThrowDoHit(EntityUid uid, ThrowableFacehuggerComponent component, ThrowDoHitEvent args)
    {
        // Only process if the facehugger was actually thrown (not just dropped)
        if (!component.IsFlying)
            return;

        // Reset flying state as the throw has completed
        component.IsFlying = false;
        var target = args.Target;

        // Only proceed if the target is a valid living entity
        if (!HasComp<MobStateComponent>(target))
            return;

        // If this is a valid facehugger entity
        if (TryComp<FaceHuggerComponent>(uid, out var faceHugger))
            // Make sure the facehugger is active before trying to attach
            faceHugger.Active = true;
    }

    #endregion
}
