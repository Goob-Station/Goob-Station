using Content.Pirate.Shared.Fluids;
using Content.Pirate.Shared.Stains.Components;
using Content.Goobstation.Common.Footprints;
using Content.Shared._Pirate.Fluids;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Fluids;
using Content.Shared.Fluids.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Standing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Pirate.Shared.Stains.Systems;

[Serializable, NetSerializable]
public enum StainVisuals : byte
{
    Volume,

    BodySlots
}

/// <summary>Raised on a stainable item when its stain amount changes.</summary>
[ByRefEvent]
public readonly record struct StainChangedEvent;

public abstract class SharedStainSystem : EntitySystem
{
    private const string FootprintSolutionName = "print";
    private const string ShoesSlot = "shoes";
    private const string GlovesSlot = "gloves";

    [Dependency] private readonly SharedSolutionContainerSystem _solution = null!;
    [Dependency] private readonly SharedPuddleSystem _puddle = null!;
    [Dependency] private readonly IPrototypeManager _proto = null!;
    [Dependency] private readonly SharedItemSystem _item = null!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly SharedHandsSystem _hands = null!;
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly InventorySystem _inventory = null!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StainableComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StainableComponent, SpilledOnEvent>(OnSpilledOn);
        // Mob spills are distributed across worn items here.
        SubscribeLocalEvent<InventoryComponent, SpilledOnEvent>(OnMobSpilledOn);
        SubscribeLocalEvent<StainableComponent, SolutionContainerChangedEvent>(OnSolutionChanged);
        SubscribeLocalEvent<StainableComponent, ReactionEntityEvent>(OnReaction);
        SubscribeLocalEvent<HandsComponent, SpilledOnEvent>(OnHandsSpilledOn);
        SubscribeLocalEvent<FootprintOwnerComponent, SpilledOnEvent>(OnFootSpilledOn);
        SubscribeLocalEvent<MeleeWeaponComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnSolutionChanged(Entity<StainableComponent> ent, ref SolutionContainerChangedEvent args)
    {
        if (args.SolutionId != ent.Comp.SolutionName)
            return;

        UpdateVisuals(ent);

        // Let other systems react without sharing SolutionContainerChangedEvent.
        var changed = new StainChangedEvent();
        RaiseLocalEvent(ent.Owner, ref changed);
    }

    private void OnMapInit(Entity<StainableComponent> ent, ref MapInitEvent args)
    {
        // Avoid empty stain solutions on items with solution-driven visuals.
        if (_solution.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out var sol))
            sol.Value.Comp.Solution.CanReact = false;
    }

    private void OnSpilledOn(Entity<StainableComponent> ent, ref SpilledOnEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<FootprintOwnerComponent>(ent.Owner) &&
            (args.TargetSlots & (SlotFlags.FEET | SlotFlags.GLOVES)) != 0)
        {
            return;
        }

        TryStain(ent, args.Solution);
    }

    // Worn clothing spills; bare skin and held items are handled separately.
    private void OnMobSpilledOn(Entity<InventoryComponent> ent, ref SpilledOnEvent args)
    {
        if (args.Handled || args.TargetSlots == SlotFlags.NONE)
            return;

        // Footwear only stains from puddle steps.
        var stainFeet = args.TargetSlots == SlotFlags.FEET && RollFeetStain(args.Source);
        StainWornItems(ent, args.Solution, args.TargetSlots, stainFeet);
    }

    /// <summary>
    /// Spreads spill stains across worn items, rotating overflow to the floor except for footwear.
    /// </summary>
    private void StainWornItems(Entity<InventoryComponent> mob, Solution spill, SlotFlags slots, bool stainFeet)
    {
        // Stain transfer and puddles are server-only.
        if (!_net.IsServer)
            return;

        var targets = new List<(Entity<StainableComponent> Ent, Entity<SolutionComponent> Sol, bool Rotate)>();
        var enumerator = _inventory.GetSlotEnumerator(mob.AsNullable(), slots);
        while (enumerator.NextItem(out var item, out var slot))
        {
            // Footwear needs the puddle-step roll.
            if ((slot.SlotFlags & SlotFlags.FEET) != 0 && !stainFeet)
                continue;

            if (!TryComp<StainableComponent>(item, out var stainable))
                continue;

            Entity<StainableComponent> ent = (item, stainable);
            if (IsStainBlocked(ent) || !TryGetStainSolution(ent, out var solComp))
                continue;

            targets.Add((ent, solComp, RotatesToFloor(slot.SlotFlags)));
        }

        if (targets.Count == 0)
            return;

        // Water washes elsewhere; it does not stain.
        var stainingVolume = spill.Volume - WaterVolume(spill);
        if (stainingVolume <= 0)
            return;

        var displaced = new Solution();
        var share = stainingVolume / targets.Count;

        foreach (var (ent, solComp, rotate) in targets)
        {
            // Footwear does not rotate overflow back onto the floor.
            var want = rotate
                ? share
                : FixedPoint2.Min(share, ent.Comp.MaxStainVolume - solComp.Comp.Solution.Volume);
            if (want <= 0)
                continue;

            var portion = spill.SplitSolutionWithout(want, "Water");
            if (portion.Volume <= 0)
                continue;

            var pushedOut = AddStainFifo(ent, solComp, portion);
            if (pushedOut.Volume > 0)
                displaced.AddSolution(pushedOut, _proto);

            EnsureComp<AppearanceComponent>(ent.Owner);
            UpdateVisuals(ent);
            OnStained(ent, solComp);
        }

        // Old stains pushed off clothing run down to the floor.
        if (displaced.Volume > 0 && !IsRunoffDrained(mob.Owner))
            _puddle.TrySpillAt(Transform(mob.Owner).Coordinates, displaced, out _, sound: false);
    }

    private bool IsRunoffDrained(EntityUid target)
    {
        return _container.TryGetContainingContainer(target, out var container)
               && HasComp<RunoffDrainComponent>(container.Owner);
    }

    // Feet do not rotate old stains back onto the floor.
    private static bool RotatesToFloor(SlotFlags slot) => (slot & SlotFlags.FEET) == 0;

    // Footwear stain chance scales with puddle depth.
    private static readonly FixedPoint2 FeetStainMinVolume = FixedPoint2.New(5);
    private const float FeetStainChancePerUnit = 0.015f;
    private const float FeetStainMaxChance = 0.30f;

    private bool RollFeetStain(EntityUid source, bool ignoreMinVolume = false)
    {
        if (!TryComp<PuddleComponent>(source, out var puddle) ||
            !_solution.TryGetSolution(source, puddle.SolutionName, out _, out var solution))
        {
            return true;
        }

        // Worn boots shrug off shallow puddles; bare feet don't get that protection.
        if (!ignoreMinVolume && solution.Volume < FeetStainMinVolume)
            return false;

        var chance = Math.Min(solution.Volume.Float() * FeetStainChancePerUnit, FeetStainMaxChance);
        return _random.Prob(chance);
    }

    private void OnHandsSpilledOn(Entity<HandsComponent> ent, ref SpilledOnEvent args)
    {
        if ((args.TargetSlots & SlotFlags.GLOVES) != 0 &&
            !_inventory.TryGetSlotEntity(ent.Owner, GlovesSlot, out _))
        {
            var stainable = EnsureComp<StainableComponent>(ent.Owner);
            stainable.BodyStainSlots |= SlotFlags.GLOVES;

            if (TryStain((ent.Owner, stainable), args.TargetSlots == SlotFlags.GLOVES ? args.Solution : args.Solution.Clone()) &&
                args.TargetSlots == SlotFlags.GLOVES)
            {
                args.Handled = true;
            }
        }

        if (!args.StainHeldItems)
            return;

        foreach (var handId in ent.Comp.Hands.Keys)
        {
            if (!_hands.TryGetHeldItem(ent.AsNullable(), handId, out var held))
                continue;

            RaiseLocalEvent(held.Value, args);

            if (args.Solution.Volume <= 0)
                break;
        }
    }

    private void OnFootSpilledOn(Entity<FootprintOwnerComponent> ent, ref SpilledOnEvent args)
    {
        if (args.Handled)
            return;

        // Bare feet only stain from puddle steps, but unlike boots they aren't shielded from shallow puddles.
        if (args.TargetSlots != SlotFlags.FEET || !RollFeetStain(args.Source, ignoreMinVolume: true))
            return;

        // Worn shoes are handled with clothing.
        if (_inventory.TryGetSlotEntity(ent.Owner, ShoesSlot, out var shoes) &&
            HasComp<StainableComponent>(shoes))
        {
            return;
        }

        var stainable = EnsureComp<StainableComponent>(ent.Owner);
        stainable.BodyStainSlots |= SlotFlags.FEET;

        if (TryStain((ent.Owner, stainable), args.Solution, rotate: false))
            args.Handled = true;
    }

    private bool TryStain(Entity<StainableComponent> ent, Solution solution, bool rotate = true)
    {
        if (IsStainBlocked(ent) || !TryGetStainSolution(ent, out var stainSolution))
            return false;

        // Take the whole dose (capped at capacity by AddStainFifo), same as worn clothing — bare
        // skin used to be throttled to a tiny amount per spill.
        var transferAmount = solution.Volume;

        // Bare feet do not rotate old stains back onto the floor.
        if (!rotate)
        {
            transferAmount = FixedPoint2.Min(transferAmount, ent.Comp.MaxStainVolume - stainSolution.Comp.Solution.Volume);
            if (transferAmount <= 0)
                return false;
        }

        var split = solution.SplitSolution(transferAmount);
        RemoveWater(split);

        if (split.Volume <= 0)
            return false;

        // Full items rotate their oldest stains out.
        var displaced = AddStainFifo(ent, stainSolution, split);

        if (_net.IsServer)
            EnsureComp<AppearanceComponent>(ent.Owner);

        UpdateVisuals(ent);
        OnStained(ent, stainSolution);

        if (displaced.Volume > 0 && !IsRunoffDrained(ent.Owner))
            _puddle.TrySpillAt(Transform(ent.Owner).Coordinates, displaced, out _, sound: false);

        return true;
    }

    /// <summary>Gets or creates the stain solution.</summary>
    private bool TryGetStainSolution(Entity<StainableComponent> ent, out Entity<SolutionComponent> solComp)
    {
        if (_solution.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out var found))
        {
            solComp = found.Value;
            return true;
        }

        if (_net.IsServer &&
            _solution.EnsureSolution(ent.Owner, ent.Comp.SolutionName, out _, ent.Comp.MaxStainVolume) &&
            _solution.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out found))
        {
            found.Value.Comp.Solution.CanReact = false;
            solComp = found.Value;
            return true;
        }

        solComp = default;
        return false;
    }

    /// <summary>
    /// Adds stains as FIFO, returning displaced reagents.
    /// </summary>
    private Solution AddStainFifo(Entity<StainableComponent> ent, Entity<SolutionComponent> solComp, Solution toAdd)
    {
        var displaced = new Solution();
        if (toAdd.Volume <= 0)
            return displaced;

        var max = ent.Comp.MaxStainVolume;

        // If incoming alone overflows, keep only its newest max units.
        if (toAdd.Volume > max)
            EvictOldest(toAdd, toAdd.Volume - max, displaced);

        // Evict oldest existing stains for the incoming.
        var overflow = solComp.Comp.Solution.Volume + toAdd.Volume - max;
        if (overflow > 0)
            EvictOldest(solComp, overflow, displaced);

        _solution.TryAddSolution(solComp, toAdd);
        return displaced;
    }

    /// <summary>Moves oldest container-solution reagents into <paramref name="into"/>.</summary>
    private void EvictOldest(Entity<SolutionComponent> solComp, FixedPoint2 amount, Solution into)
    {
        EvictOldest(solComp.Comp.Solution, amount, into,
            (reagent, take) => _solution.RemoveReagent(solComp, reagent, take));
    }

    /// <summary>Moves oldest solution reagents into <paramref name="into"/>.</summary>
    private static void EvictOldest(Solution source, FixedPoint2 amount, Solution into)
    {
        EvictOldest(source, amount, into, (reagent, take) => source.RemoveReagent(reagent, take));
    }

    private static void EvictOldest(Solution from, FixedPoint2 amount, Solution into, Func<ReagentId, FixedPoint2, FixedPoint2> remove)
    {
        var remaining = amount;
        foreach (var content in new List<ReagentQuantity>(from.Contents))
        {
            if (remaining <= 0)
                break;

            var take = FixedPoint2.Min(remaining, content.Quantity);
            var removed = remove(content.Reagent, take);
            into.AddReagent(content.Reagent, removed);
            remaining -= removed;
        }
    }

    private static void RemoveWater(Solution solution)
    {
        for (var i = solution.Contents.Count - 1; i >= 0; i--)
        {
            if (solution.Contents[i].Reagent.Prototype == "Water")
                solution.RemoveReagent(solution.Contents[i].Reagent, solution.Contents[i].Quantity);
        }
    }

    private static FixedPoint2 WaterVolume(Solution solution)
    {
        var total = FixedPoint2.Zero;
        foreach (var content in solution.Contents)
        {
            if (content.Reagent.Prototype == "Water")
                total += content.Quantity;
        }

        return total;
    }

    protected virtual void OnStained(Entity<StainableComponent> ent, Entity<SolutionComponent> solution)
    {
    }

    protected virtual void OnCleaned(Entity<StainableComponent> ent)
    {
    }

    private void OnReaction(Entity<StainableComponent> ent, ref ReactionEntityEvent args)
    {
        if (args.Method != ReactionMethod.Touch || !IsCleaningReagent(args.Reagent.ID))
            return;

        TryCleanStain(ent.Owner);
    }

    // Melee blood splatter scales with the hit's damage, down to a floor.
    private const float MeleeBloodPerDamage = 0.1f;
    private static readonly FixedPoint2 MinMeleeBlood = FixedPoint2.New(1);

    private void OnMeleeHit(Entity<MeleeWeaponComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit ||
            args.Weapon == args.User ||
            args.HitEntities.Count == 0 ||
            args.BaseDamage.GetTotal() <= 0)
        {
            return;
        }

        var damage = args.BaseDamage.GetTotal();
        if (!_random.Prob(Math.Clamp((25f + damage.Float() * 2f) / 100f, 0f, 1f)))
            return;

        // Bigger hits splatter more blood; small hits still draw at least the floor amount.
        var bloodPerTarget = FixedPoint2.Max(MinMeleeBlood, damage * MeleeBloodPerDamage);

        var solution = new Solution();

        foreach (var target in args.HitEntities)
        {
            if (!TryComp<BloodstreamComponent>(target, out var bloodstream))
                continue;

            if (bloodstream.BloodReferenceSolution.Contents.Count == 0)
                continue;

            // Preserve the target's blood type.
            var bloodReagent = bloodstream.BloodReferenceSolution.Contents[0].Reagent.Prototype;
            solution.AddReagent(new ReagentId(bloodReagent, _bloodstream.GetEntityBloodData(target)), bloodPerTarget);
        }

        if (solution.Volume <= 0)
            return;

        var stainable = EnsureComp<StainableComponent>(ent.Owner);
        TryStain((ent.Owner, stainable), solution.Clone());

        // Stronger hits can splatter more attacker slots.
        var bloodiedSlots = SlotFlags.GLOVES;
        if (damage >= 20 || damage >= 15 && _random.Prob(0.25f))
        {
            bloodiedSlots |= SlotFlags.INNERCLOTHING | SlotFlags.OUTERCLOTHING;
            if (_random.Prob(0.33f))
                bloodiedSlots |= SlotFlags.FEET;
            if (damage >= 24 && _random.Prob(0.33f))
                bloodiedSlots |= SlotFlags.MASK;
            if (damage >= 30 && _random.Prob(0.33f))
                bloodiedSlots |= SlotFlags.HEAD;
        }

        // Let empty hand/foot slots stain bare limbs.
        RaiseLocalEvent(args.User, new SpilledOnEvent(args.User, solution.Clone(), bloodiedSlots));
        StainVictimSlots(args.User, args.HitEntities, solution);
    }

    private void StainEquippedSlots(EntityUid uid, Solution source, SlotFlags slots)
    {
        if (!_inventory.TryGetContainerSlotEnumerator(uid, out var enumerator, slots))
            return;

        while (enumerator.NextItem(out var item))
        {
            if (!TryComp<StainableComponent>(item, out var stainable))
                continue;

            TryStain((item, stainable), source.Clone());
        }
    }

    private void StainVictimSlots(EntityUid user, IReadOnlyList<EntityUid> targets, Solution source)
    {
        var targetPart = TryComp<TargetingComponent>(user, out var targeting)
            ? PickHitPart(targeting)
            : TargetBodyPart.Chest;

        var victimSlots = targetPart switch
        {
            TargetBodyPart.Head => SlotFlags.MASK | SlotFlags.HEAD | (_random.Prob(0.33f) ? SlotFlags.EYES : SlotFlags.NONE),
            TargetBodyPart.Chest => SlotFlags.INNERCLOTHING | SlotFlags.OUTERCLOTHING,
            _ => SlotFlags.NONE
        };

        if (victimSlots == SlotFlags.NONE)
            return;

        foreach (var target in targets)
            StainEquippedSlots(target, source, victimSlots);
    }

    private TargetBodyPart PickHitPart(TargetingComponent targeting)
    {
        if (!targeting.TargetOdds.TryGetValue(targeting.Target, out var odds))
            return targeting.Target;

        var roll = _random.NextFloat();
        var total = 0f;

        foreach (var (part, chance) in odds)
        {
            total += chance;
            if (roll <= total)
                return part;
        }

        return targeting.Target;
    }

    private bool IsStainBlocked(Entity<StainableComponent> ent)
    {
        if (!_container.TryGetContainingContainer(ent.Owner, out var container) ||
            !TryComp<InventoryComponent>(container.Owner, out var inv))
        {
            return false;
        }

        if (!_inventory.TryGetSlot(container.Owner, container.ID, out var slotDef, inv))
            return false;

        foreach (var slot in inv.Slots)
        {
            if (!_inventory.TryGetSlotEntity(container.Owner, slot.Name, out var slotEnt, inv))
                continue;

            if (TryComp<StainBlockerComponent>(slotEnt, out var blocker) &&
                (blocker.BlockedSlots & slotDef.SlotFlags) != 0)
            {
                return true;
            }
        }

        return false;
    }

    public bool TryCleanStain(EntityUid uid)
    {
        if (!TryComp<StainableComponent>(uid, out var stainable) ||
            !_solution.TryGetSolution(uid, stainable.SolutionName, out var solComp, out var sol) ||
            sol.Volume <= 0)
        {
            return false;
        }

        stainable.BodyStainSlots = SlotFlags.NONE;
        _solution.RemoveAllSolution(solComp.Value);
        UpdateVisuals((uid, stainable));
        OnCleaned((uid, stainable));
        return true;
    }

    /// <summary>Whether the entity currently carries any stain.</summary>
    public bool HasStain(EntityUid uid)
    {
        return TryComp<StainableComponent>(uid, out var stainable) &&
               _solution.TryGetSolution(uid, stainable.SolutionName, out _, out var sol) &&
               sol.Volume > FixedPoint2.Zero;
    }

    /// <summary>
    /// Cleans one bare-body stain slot, emptying the solution when no body stains remain.
    /// </summary>
    public bool TryCleanBodyStain(EntityUid uid, SlotFlags slot)
    {
        if (!TryComp<StainableComponent>(uid, out var stainable) || (stainable.BodyStainSlots & slot) == 0)
            return false;

        stainable.BodyStainSlots &= ~slot;

        if (stainable.BodyStainSlots == SlotFlags.NONE &&
            _solution.TryGetSolution(uid, stainable.SolutionName, out var solComp, out _))
        {
            _solution.RemoveAllSolution(solComp.Value);
            OnCleaned((uid, stainable));
        }

        UpdateVisuals((uid, stainable));
        return true;
    }

    public bool CleanEntityAndEquipment(EntityUid uid)
    {
        var cleaned = false;
        var seen = new HashSet<EntityUid>();

        cleaned |= TryCleanStain(uid);
        seen.Add(uid);

        if (_inventory.TryGetContainerSlotEnumerator(uid, out var enumerator, SlotFlags.WITHOUT_POCKET))
        {
            while (enumerator.NextItem(out var item))
                cleaned |= TryCleanSeen(item, seen);
        }

        if (TryComp<HandsComponent>(uid, out var hands))
        {
            foreach (var held in _hands.EnumerateHeld((uid, hands)))
                cleaned |= TryCleanSeen(held, seen);
        }

        cleaned |= TryCleanFootprints(uid);

        return cleaned;
    }

    private bool TryCleanSeen(EntityUid uid, HashSet<EntityUid> seen)
    {
        if (!seen.Add(uid))
            return false;

        return TryCleanStain(uid);
    }

    private bool TryCleanFootprints(EntityUid uid)
    {
        if (!HasComp<FootprintOwnerComponent>(uid) ||
            !_solution.TryGetSolution(uid, FootprintSolutionName, out var solComp, out var sol) ||
            sol.Volume <= 0)
        {
            return false;
        }

        _solution.RemoveAllSolution(solComp.Value);
        return true;
    }

    private static bool IsCleaningReagent(string reagent)
    {
        // Plain water dilutes stains through wetness.
        return reagent is "SoapReagent" or "SpaceCleaner";
    }

    /// <summary>Current stain volume, or zero.</summary>
    public FixedPoint2 GetStainVolume(EntityUid uid)
    {
        return TryComp<StainableComponent>(uid, out var stainable) &&
               _solution.TryGetSolution(uid, stainable.SolutionName, out _, out var sol)
            ? sol.Volume
            : FixedPoint2.Zero;
    }

    /// <summary>
    /// Washes stain into runoff.
    /// </summary>
    public Solution? WashStain(EntityUid uid, FixedPoint2 amount)
    {
        if (amount <= 0 ||
            !TryComp<StainableComponent>(uid, out var stainable) ||
            !_solution.TryGetSolution(uid, stainable.SolutionName, out var solComp, out var sol) ||
            sol.Volume <= 0)
        {
            return null;
        }

        var removed = _solution.SplitSolution(solComp.Value, FixedPoint2.Min(amount, sol.Volume));

        var ent = (uid, stainable);
        if (sol.Volume <= 0)
        {
            stainable.BodyStainSlots = SlotFlags.NONE;
            OnCleaned(ent);
        }

        UpdateVisuals(ent);
        return removed;
    }

    public void UpdateVisuals(Entity<StainableComponent> ent)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (!TryComp<AppearanceComponent>(ent.Owner, out var app))
            return;

        var volume = _solution.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out _, out var sol)
            ? sol.Volume
            : FixedPoint2.Zero;
        var slots = ent.Comp.BodyStainSlots;

        if (_appearance.TryGetData(ent.Owner, StainVisuals.Volume, out FixedPoint2 prevVolume, app)
            && prevVolume == volume
            && _appearance.TryGetData(ent.Owner, StainVisuals.BodySlots, out SlotFlags prevSlots, app)
            && prevSlots == slots)
        {
            return;
        }

        _appearance.SetData(ent.Owner, StainVisuals.Volume, volume, app);
        _appearance.SetData(ent.Owner, StainVisuals.BodySlots, slots, app);
        _item.VisualsChanged(ent.Owner);

        if (_container.TryGetContainingContainer(ent.Owner, out var container) &&
            TryComp<AppearanceComponent>(container.Owner, out var wearerApp))
        {
            _appearance.QueueUpdate(container.Owner, wearerApp);
            Dirty(container.Owner, wearerApp);
        }
    }

}
