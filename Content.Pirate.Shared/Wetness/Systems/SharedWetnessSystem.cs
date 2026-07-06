using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Clothing.Components;
using Content.Pirate.Shared.Fluids;
using Content.Pirate.Shared.Stains.Systems;
using Content.Pirate.Shared.Wetness.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Fluids;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Pirate.Shared.Wetness.Systems;

/// <summary>
/// Raised on a wettable item (server only) whenever its wetness changes, so the mood roll-up can react.
/// </summary>
[ByRefEvent]
public readonly record struct WetnessChangedEvent;

/// <summary>
/// Wetness core: clean-water absorption tracked separately from stains. Deliberately mirrors
/// <see cref="SharedStainSystem"/> so the two stay reviewable side by side. State is
/// server-authoritative; only visuals/sound react to the replicated value.
/// </summary>
public abstract class SharedWetnessSystem : EntitySystem
{
    private const string WaterReagent = "Water";

    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly InventorySystem _inventory = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedPuddleSystem _puddle = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedStainSystem _stains = null!;
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IPrototypeManager _proto = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] protected readonly IGameTiming Timing = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WettableComponent, ReactionEntityEvent>(OnReaction);
        SubscribeLocalEvent<InventoryComponent, ReactionEntityEvent>(OnMobReaction);
        SubscribeLocalEvent<WettableComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<WettableComponent, WringWetnessDoAfterEvent>(OnWring);
        SubscribeLocalEvent<WettableComponent, ExaminedEvent>(OnExamined);

        // Keep the wearer droplet flag correct as wet clothing is put on and taken off.
        SubscribeLocalEvent<InventoryComponent, DidEquipEvent>(OnDidEquip);
        SubscribeLocalEvent<InventoryComponent, DidUnequipEvent>(OnDidUnequip);
    }

    private void OnDidEquip(Entity<InventoryComponent> ent, ref DidEquipEvent args)
    {
        UpdateWearerVisuals(ent.Owner);
    }

    private void OnDidUnequip(Entity<InventoryComponent> ent, ref DidUnequipEvent args)
    {
        UpdateWearerVisuals(ent.Owner);
    }

    private void OnExamined(Entity<WettableComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Wetness <= 0)
            return;

        var ratio = ent.Comp.MaxWetness > 0 ? (ent.Comp.Wetness / ent.Comp.MaxWetness).Float() : 1f;
        var level = ratio >= 0.66f ? "soaked" : ratio >= 0.33f ? "wet" : "damp";
        args.PushMarkup(Loc.GetString($"wetness-examine-{level}", ("item", ent.Owner)));
    }

    #region Public API

    /// <summary>Absorbs water into a wettable item, respecting blockers and capacity.</summary>
    public void AddWetness(Entity<WettableComponent> ent, FixedPoint2 amount)
    {
        if (amount <= 0 || IsWetBlocked(ent))
            return;

        var newWetness = FixedPoint2.Min(ent.Comp.Wetness + amount, ent.Comp.MaxWetness);
        if (newWetness == ent.Comp.Wetness)
            return;

        var wasWet = ent.Comp.Wetness > 0;
        ent.Comp.Wetness = newWetness;

        // Server drives the drying schedule; the field is networked so clients mirror it.
        if (!wasWet && _net.IsServer)
            ent.Comp.NextDryTime = Timing.CurTime + NextDryDelay(ent.Comp);

        Dirty(ent);
        NotifyChanged(ent);
        UpdateVisuals(ent);
    }

    /// <summary>Removes up to <paramref name="amount"/> wetness; returns how much was actually removed.</summary>
    public FixedPoint2 RemoveWetness(Entity<WettableComponent> ent, FixedPoint2 amount)
    {
        var removed = FixedPoint2.Min(amount, ent.Comp.Wetness);
        if (removed <= 0)
            return FixedPoint2.Zero;

        ent.Comp.Wetness -= removed;
        Dirty(ent);
        NotifyChanged(ent);
        UpdateVisuals(ent);
        return removed;
    }

    /// <summary>Instantly dries an item (dryers, washing-machine cycle end).</summary>
    public void DryFully(Entity<WettableComponent> ent)
    {
        RemoveWetness(ent, ent.Comp.Wetness);
    }

    /// <summary>Collects the items worn in the given slots.</summary>
    private List<EntityUid> GetWornItems(EntityUid mob, SlotFlags slots)
    {
        var items = new List<EntityUid>();
        if (_inventory.TryGetContainerSlotEnumerator(mob, out var enumerator, slots))
        {
            while (enumerator.NextItem(out var item))
                items.Add(item);
        }

        return items;
    }

    public bool IsWet(EntityUid uid)
    {
        return TryComp<WettableComponent>(uid, out var wettable) && wettable.Wetness > 0;
    }

    /// <summary>
    /// Immersion in water (e.g. wading into floor water): flows <paramref name="flow"/> units over the
    /// mob so it both soaks worn clothing and washes stains off it (same as any water contact), but the
    /// dirty runoff is drained away instead of pooled. A large flow fully soaks and rinses.
    /// </summary>
    public void ImmerseInWater(EntityUid mob, FixedPoint2 flow)
    {
        var worn = GetWornItems(mob, SlotFlags.WITHOUT_POCKET);
        var washTargets = new List<EntityUid>(worn) { mob };
        ApplyWater(mob, flow, worn, washTargets, drainRunoff: true);
    }

    #endregion

    #region Water contact

    private void OnReaction(Entity<WettableComponent> ent, ref ReactionEntityEvent args)
    {
        if (args.Method != ReactionMethod.Touch || args.Reagent.ID != WaterReagent)
            return;

        // A loose item soaks up what it can and washes its own stains; the rest pools beneath it.
        var single = new List<EntityUid> { ent.Owner };
        ApplyWater(ent.Owner, args.ReagentQuantity.Quantity, single, single);
    }

    // Owns the mob-level reagent touch for both wetness and stains (a (component, event) pair can
    // only have one directed subscription, so the stain system delegates SpaceCleaner cleaning here).
    private void OnMobReaction(Entity<InventoryComponent> ent, ref ReactionEntityEvent args)
    {
        if (args.Method != ReactionMethod.Touch)
            return;

        switch (args.Reagent.ID)
        {
            case WaterReagent:
                var worn = GetWornItems(ent.Owner, SlotFlags.WITHOUT_POCKET);
                // Wetting spreads over worn clothing; washing also covers bare-body stains.
                var washTargets = new List<EntityUid>(worn) { ent.Owner };
                ApplyWater(ent.Owner, args.ReagentQuantity.Quantity, worn, washTargets);
                break;
            case "SpaceCleaner":
                _stains.CleanEntityAndEquipment(ent.Owner);
                break;
        }
    }

    /// <summary>
    /// Applies a water contact of <paramref name="water"/> units at <paramref name="at"/>: it soaks
    /// into <paramref name="wetTargets"/> (spread evenly, overflowing from full items onto the rest)
    /// and rinses stains off <paramref name="washTargets"/> (1u water per 1u stain, spread evenly).
    /// The water that can't soak in anywhere, together with the washed-out stains, pools below as a
    /// mixture — so a shower leaves, e.g., a red puddle while it rinses blood off someone.
    /// </summary>
    private void ApplyWater(EntityUid at, FixedPoint2 water, List<EntityUid> wetTargets, List<EntityUid> washTargets, bool drainRunoff = false)
    {
        // Wetness, stain removal, and puddles are all server-authoritative.
        if (water <= 0 || !_net.IsServer)
            return;

        var absorbed = DistributeWetness(wetTargets, water);
        var runoff = CollectWashedStains(washTargets, water);

        // "Excessive water that can't get on us" runs off with the washed-out stains.
        var excess = water - absorbed;
        if (excess > 0)
            runoff.AddReagent(WaterReagent, excess);

        // Pool the runoff on the floor, unless it should drain away instead: the caller asked for it
        // (immersion in floor water) or the target is inside a draining container (washing machine).
        if (runoff.Volume > 0 && !drainRunoff && !IsRunoffDrained(at))
            _puddle.TrySpillAt(Transform(at).Coordinates, runoff, out _, sound: false);
    }

    /// <summary>True when the target is inside a container that drains liquid runoff (e.g. a washing machine).</summary>
    private bool IsRunoffDrained(EntityUid target)
    {
        return _container.TryGetContainingContainer(target, out var container)
               && HasComp<RunoffDrainComponent>(container.Owner);
    }

    /// <summary>
    /// Spreads <paramref name="water"/> evenly across the wettable, unblocked items among
    /// <paramref name="targets"/>, cascading each full item's share onto the ones that still have
    /// spare capacity. Returns how much water was actually absorbed.
    /// </summary>
    private FixedPoint2 DistributeWetness(List<EntityUid> targets, FixedPoint2 water)
    {
        // Items that can still take water, with their remaining capacity and running allocation.
        var items = new List<(Entity<WettableComponent> Ent, FixedPoint2 Cap, FixedPoint2 Alloc)>();
        foreach (var target in targets)
        {
            if (!TryComp<WettableComponent>(target, out var wettable))
                continue;

            Entity<WettableComponent> ent = (target, wettable);
            if (IsWetBlocked(ent))
                continue;

            var capacity = wettable.MaxWetness - wettable.Wetness;
            if (capacity > 0)
                items.Add((ent, capacity, FixedPoint2.Zero));
        }

        var remaining = water;
        var progressed = true;
        while (remaining > 0 && progressed)
        {
            progressed = false;

            var open = 0;
            foreach (var item in items)
            {
                if (item.Alloc < item.Cap)
                    open++;
            }

            if (open == 0)
                break;

            var share = remaining / open;
            if (share <= 0)
                break;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var free = item.Cap - item.Alloc;
                if (free <= 0)
                    continue;

                var give = FixedPoint2.Min(share, free);
                if (give <= 0)
                    continue;

                items[i] = (item.Ent, item.Cap, item.Alloc + give);
                remaining -= give;
                progressed = true;
            }
        }

        var absorbed = FixedPoint2.Zero;
        foreach (var item in items)
        {
            if (item.Alloc <= 0)
                continue;

            AddWetness(item.Ent, item.Alloc);
            absorbed += item.Alloc;
        }

        return absorbed;
    }

    /// <summary>
    /// Spreads <paramref name="water"/> evenly across whichever targets are actually stained (1u
    /// water removes 1u stain) and returns the washed-out reagents.
    /// </summary>
    private Solution CollectWashedStains(List<EntityUid> targets, FixedPoint2 water)
    {
        var stained = new List<EntityUid>();
        foreach (var target in targets)
        {
            if (_stains.GetStainVolume(target) > 0)
                stained.Add(target);
        }

        var runoff = new Solution();
        if (stained.Count == 0)
            return runoff;

        var share = water / stained.Count;
        foreach (var item in stained)
        {
            var washed = _stains.WashStain(item, share);
            if (washed != null)
                runoff.AddSolution(washed, _proto);
        }

        return runoff;
    }

    #endregion

    #region Blocking

    /// <summary>
    /// Mirrors <see cref="SharedStainSystem"/> blocking: walk the wearer's other worn items and
    /// stop if any covers this item's slot. Modsuit blockers only count while sealed.
    /// </summary>
    protected bool IsWetBlocked(Entity<WettableComponent> ent)
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

            if (!TryComp<WetnessBlockerComponent>(slotEnt, out var blocker) ||
                (blocker.BlockedSlots & slotDef.SlotFlags) == 0)
            {
                continue;
            }

            // Sealed-gated blockers (modsuits) only block once their suit is sealed.
            if (blocker.RequiresSealed &&
                (!TryComp<SealableClothingComponent>(slotEnt, out var sealable) || !sealable.IsSealed))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    #endregion

    #region Visuals

    /// <summary>
    /// Rolls up worn wettable items onto the WEARER's appearance so the client can draw the
    /// droplet overlay in response to the replicated state (see <see cref="SharedStainSystem.UpdateVisuals"/>).
    /// </summary>
    public void UpdateVisuals(Entity<WettableComponent> ent)
    {
        // Only worn clothing shows the wearer droplet effect.
        if (_container.TryGetContainingContainer(ent.Owner, out var container))
            UpdateWearerVisuals(container.Owner);
    }

    /// <summary>
    /// Rolls up every worn wettable item onto the wearer's droplet marker. Server-authoritative:
    /// the client draws the overlay purely from the replicated <see cref="WetVisualsComponent"/>.
    /// </summary>
    public void UpdateWearerVisuals(EntityUid wearer)
    {
        if (!_net.IsServer)
            return;

        if (AnyWornWetAboveThreshold(wearer))
            EnsureComp<WetVisualsComponent>(wearer);
        else
            RemComp<WetVisualsComponent>(wearer);
    }

    private bool AnyWornWetAboveThreshold(EntityUid wearer)
    {
        if (!_inventory.TryGetContainerSlotEnumerator(wearer, out var enumerator, SlotFlags.WITHOUT_POCKET))
            return false;

        while (enumerator.NextItem(out var item))
        {
            if (TryComp<WettableComponent>(item, out var wettable) && wettable.Wetness >= wettable.VisualThreshold)
                return true;
        }

        return false;
    }

    #endregion

    #region Wringing

    private void OnGetVerbs(Entity<WettableComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanInteract || !args.CanAccess || args.Using != ent.Owner || ent.Comp.Wetness <= 0)
            return;

        var user = args.User;
        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString("wetness-verb-wring"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/bubbles.svg.192dpi.png")),
            Act = () =>
            {
                if (_doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.WringDoAfterDuration, new WringWetnessDoAfterEvent(), ent.Owner)
                    {
                        BreakOnMove = true,
                        BreakOnDamage = true,
                        NeedHand = true
                    }))
                {
                    _audio.PlayPredicted(ent.Comp.WringSound, ent.Owner, user);
                }
            }
        });
    }

    private void OnWring(Entity<WettableComponent> ent, ref WringWetnessDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || ent.Comp.Wetness <= 0)
            return;

        // Wetness mutation and spilling are server-authoritative.
        if (!_net.IsServer)
            return;

        // Wringing only dumps the clothing's clean water. Stains stay in the item.
        var water = new Solution();
        water.AddReagent(WaterReagent, ent.Comp.Wetness);

        // Don't consume the DoAfter or clear the item's wetness if the spill couldn't happen.
        if (!_puddle.TrySpillAt(args.User, water, out _))
            return;

        args.Handled = true;
        ent.Comp.Wetness = FixedPoint2.Zero;
        Dirty(ent);
        NotifyChanged(ent);
        UpdateVisuals(ent);
        _popup.PopupEntity(Loc.GetString("wetness-verb-wring-success"), args.User, args.User);
    }

    #endregion

    protected TimeSpan NextDryDelay(WettableComponent comp)
    {
        return TimeSpan.FromSeconds(_random.NextFloat(comp.DryIntervalMin, comp.DryIntervalMax));
    }

    private void NotifyChanged(Entity<WettableComponent> ent)
    {
        if (!_net.IsServer)
            return;

        var ev = new WetnessChangedEvent();
        RaiseLocalEvent(ent.Owner, ref ev);
    }
}
