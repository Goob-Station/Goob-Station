using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Clothing.Components;
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

    /// <summary>Wets every wettable item worn in the given slots (showers, splashes, deep water).</summary>
    public void WetEquippedSlots(EntityUid mob, SlotFlags slots, FixedPoint2 amount)
    {
        if (!_inventory.TryGetContainerSlotEnumerator(mob, out var enumerator, slots))
            return;

        while (enumerator.NextItem(out var item))
        {
            if (TryComp<WettableComponent>(item, out var wettable))
                AddWetness((item, wettable), amount);

            // The same water event that wets also washes out stains.
            _stains.DiluteStains(item, amount);
        }
    }

    public bool IsWet(EntityUid uid)
    {
        return TryComp<WettableComponent>(uid, out var wettable) && wettable.Wetness > 0;
    }

    #endregion

    #region Water contact

    private void OnReaction(Entity<WettableComponent> ent, ref ReactionEntityEvent args)
    {
        if (args.Method != ReactionMethod.Touch || args.Reagent.ID != WaterReagent)
            return;

        var volume = args.ReagentQuantity.Quantity;
        AddWetness(ent, volume);
        _stains.DiluteStains(ent.Owner, volume);
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
                var volume = args.ReagentQuantity.Quantity;
                WetEquippedSlots(ent.Owner, SlotFlags.WITHOUT_POCKET, volume);
                // Bare-body stains (feet/hands) live on the mob itself.
                _stains.DiluteStains(ent.Owner, volume);
                break;
            case "SpaceCleaner":
                _stains.CleanEntityAndEquipment(ent.Owner);
                break;
        }
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
        args.Handled = true;

        // Wringing only dumps the clothing's clean water. Stains stay in the item.
        var water = new Solution();
        water.AddReagent(WaterReagent, ent.Comp.Wetness);

        if (!_puddle.TrySpillAt(args.User, water, out _))
            return;

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
