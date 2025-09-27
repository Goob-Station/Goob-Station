using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared.Atmos.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
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
using Content.Shared.Body.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Server._White.Xenomorphs.FaceHugger;

public sealed class FaceHuggerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReactiveSystem _reactiveSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;

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
    }

    private void OnCollideEvent(EntityUid uid, FaceHuggerComponent component, StartCollideEvent args)
        => TryEquipFaceHugger(uid, args.OtherEntity, component);

    private void OnMeleeHit(EntityUid uid, FaceHuggerComponent component, MeleeHitEvent args)
    {
        if (args.HitEntities.FirstOrNull() is not {} target)
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
        _popup.PopupEntity(Loc.GetString("xenomorphs-face-hugger-equip-other", ("equipment", uid), ("target", Identity.Entity(args.Equipee, EntityManager))), uid, Filter.PvsExcept(args.Equipee), true);

        _stun.TryKnockdown(args.Equipee, component.KnockdownTime, true);

        if (component.InfectionPrototype.HasValue)
            EnsureComp<XenomorphPreventSuicideComponent>(args.Equipee); //Prevent suicide for infected

        if (!component.InfectionPrototype.HasValue)
            return;

        component.InfectIn = _timing.CurTime + _random.Next(component.MinInfectTime, component.MaxInfectTime);
    }

    private void OnBeingUnequippedAttempt(EntityUid uid, FaceHuggerComponent component, BeingUnequippedAttemptEvent args)
    {
        if (component.Slot != args.Slot || args.Unequipee != args.UnEquipTarget || !component.InfectionPrototype.HasValue || _mobState.IsDead(uid))
            return;

        _popup.PopupEntity(Loc.GetString("xenomorphs-face-hugger-unequip", ("equipment", Identity.Entity(uid, EntityManager))), uid, args.Unequipee);
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
            if (TryComp<ClothingComponent>(uid, out var clothing) && clothing.InSlot != null)
            {
                // Initialize NextInjectionTime if it's zero
                if (faceHugger.NextInjectionTime == TimeSpan.Zero)
                {
                    faceHugger.NextInjectionTime = time + faceHugger.InitialInjectionDelay;
                    Log.Debug($"[FaceHugger] First injection scheduled for {faceHugger.NextInjectionTime} (initial delay: {faceHugger.InitialInjectionDelay.TotalSeconds}s)");
                    continue;
                }

                if (time >= faceHugger.NextInjectionTime)
                {
                    // Get the entity that has this item equipped
                    if (_container.TryGetContainingContainer(uid, out var container) && container.Owner != uid)
                    {
                        Log.Debug($"[FaceHugger] Time for injection at {time}");
                        InjectChemicals(uid, faceHugger, container.Owner);
                        // Set the next injection time based on the current time plus interval
                        faceHugger.NextInjectionTime = time + faceHugger.InjectionInterval;
                        Log.Debug($"[FaceHugger] Next injection scheduled for {faceHugger.NextInjectionTime}");
                    }
                }
            }

            // Check for nearby entities to latch onto
            if (faceHugger.Active && clothing?.InSlot == null)
            {
                foreach (var entity in _entityLookup.GetEntitiesInRange<InventoryComponent>(Transform(uid).Coordinates, 1.5f))
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

        var bodyPart = _body.GetBodyChildrenOfType(target.Owner, component.InfectionBodyPart.Type, symmetry: component.InfectionBodyPart.Symmetry).FirstOrNull();
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

        // Set the rest time and deactivate
        var restTime = _random.Next(component.MinRestTime, component.MaxRestTime);
        component.RestIn = _timing.CurTime + restTime;
        component.Active = false;
        Log.Debug($"[FaceHugger] Facehugger deactivated. Will reactivate in {restTime.TotalSeconds:0.##} seconds");

        // Check for any blocking masks or equipment
        if (CheckAndHandleMask(target, out var blocker))
        {
            _audio.PlayPvs(component.SoundOnImpact, uid);
            _damageable.TryChangeDamage(uid, component.DamageOnImpact);
            _popup.PopupEntity(Loc.GetString("xenomorphs-face-hugger-try-equip", ("equipment", uid), ("equipmentBlocker", blocker!.Value)), uid);
            _popup.PopupEntity(Loc.GetString("xenomorphs-face-hugger-try-equip-other",
                ("equipment", uid),
                ("equipmentBlocker", blocker.Value),
                ("target", Identity.Entity(target, EntityManager))),
                uid, Filter.PvsExcept(target), true);

            return false;
        }

        return _inventory.TryEquip(target, uid, component.Slot, true, true);
    }

    #region Injection Code
    /// <summary>
    /// Checks if the facehugger can inject chemicals into the target
    /// </summary>
    public bool CanInject(EntityUid uid, FaceHuggerComponent component, EntityUid target)
    {
        // Check if facehugger is properly equipped
        if (!TryComp<ClothingComponent>(uid, out var clothingComp) || clothingComp.InSlot == null)
        {
            if (!component.Active)
            {
                Log.Debug("[FaceHugger] Cannot inject - Facehugger is not active and not equipped");
                return false;
            }
            return true;
        }

        // Check if target already has the sleep chemical
        if (TryComp<BloodstreamComponent>(target, out var bloodstream) &&
            _solutions.ResolveSolution(target, bloodstream.ChemicalSolutionName, ref bloodstream.ChemicalSolution, out var chemSolution) &&
            chemSolution.TryGetReagentQuantity(new ReagentId(component.SleepChem, null), out var quantity) &&
            quantity > FixedPoint2.New(component.MinChemicalThreshold))
        {
            Log.Debug($"[FaceHugger] {ToPrettyString(target)} already has {quantity}u of {component.SleepChem}");
            return false;
        }

        Log.Debug($"[FaceHugger] Facehugger is equipped in slot {clothingComp.InSlot}, allowing injection");
        return true;
    }

    /// <summary>
    /// Creates a solution with the sleep chemical
    /// </summary>
    public Solution CreateSleepChemicalSolution(FaceHuggerComponent component, float amount)
    {
        var solution = new Solution();
        solution.AddReagent(component.SleepChem, amount);
        Log.Debug($"[FaceHugger] Created sleep chemical solution: {solution} with {amount}u of {component.SleepChem}");
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

        Log.Debug($"[FaceHugger] Successfully injected {chemAmount}u of {chemName} into bloodstream");
        _reactiveSystem.DoEntityReaction(target, solution, ReactionMethod.Injection);
        return true;
    }

    /// <summary>
    /// Main method to handle chemical injection
    /// </summary>
    public void InjectChemicals(EntityUid uid, FaceHuggerComponent component, EntityUid target)
    {
        if (!CanInject(uid, component, target))
        {
            Log.Debug($"[FaceHugger] Injection failed for {ToPrettyString(target)}");
            return;
        }

        Log.Debug($"[FaceHugger] Attempting to inject {component.SleepChemAmount}u of {component.SleepChem} into {ToPrettyString(target)}");

        var sleepChem = CreateSleepChemicalSolution(component, component.SleepChemAmount);
        if (!TryInjectIntoBloodstream(target, sleepChem, component.SleepChem, component.SleepChemAmount))
        {
            Log.Warning($"[FaceHugger] Failed to inject {component.SleepChem} into {ToPrettyString(target)}");
        }
    }
    #endregion

    #region Handle Face Masks
    /// <summary>
    /// Checks if the target has a breathable mask or any other blocking equipment.
    /// Returns true if there's a blocker, false otherwise.
    /// </summary>
    private bool CheckAndHandleMask(EntityUid target, out EntityUid? blocker)
    {
        blocker = null;

        // Check for breathable mask
        if (_inventory.TryGetSlotEntity(target, "mask", out var maskUid))
        {
            // If the mask is a breath tool (gas mask) and is functional, block the facehugger
            if (TryComp<BreathToolComponent>(maskUid, out var breathTool) && breathTool.IsFunctional)
            {
                blocker = maskUid;
                return true;
            }
            // If it's just a regular mask, remove it
            else
            {
                _inventory.TryUnequip(target, "mask", true);
            }
        }

        return false;
    }
    #endregion
}
