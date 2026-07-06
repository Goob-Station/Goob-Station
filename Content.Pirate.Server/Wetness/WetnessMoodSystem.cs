using Content.Pirate.Server.Mood;
using Content.Pirate.Shared.Stains.Components;
using Content.Pirate.Shared.Stains.Systems;
using Content.Pirate.Shared.Wetness.Components;
using Content.Pirate.Shared.Wetness.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mood;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Pirate.Server.Wetness;

/// <summary>
/// Applies wet, dirty, and fresh-laundry mood effects.
/// </summary>
public sealed class WetnessMoodSystem : EntitySystem
{
    private const string WetMoodId = "WetClothing";
    private const string DirtyMoodId = "DirtyClothing";
    private const string FreshLaundryMoodId = "FreshLaundry";

    [Dependency] private readonly InventorySystem _inventory = null!;
    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly SharedStainSystem _stains = null!;
    [Dependency] private readonly SharedWetnessSystem _wetness = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MoodComponent, DidEquipEvent>(OnEquip);
        SubscribeLocalEvent<MoodComponent, DidUnequipEvent>(OnUnequip);
        SubscribeLocalEvent<WettableComponent, WetnessChangedEvent>(OnWetnessChanged);
        SubscribeLocalEvent<StainableComponent, StainChangedEvent>(OnStainChanged);
    }

    private void OnEquip(Entity<MoodComponent> ent, ref DidEquipEvent args)
    {
        Recompute(ent.Owner);
        TryGrantFreshLaundry(ent.Owner, args.Equipment, args.SlotFlags);
    }

    private void OnUnequip(Entity<MoodComponent> ent, ref DidUnequipEvent args)
    {
        Recompute(ent.Owner);
    }

    private void OnWetnessChanged(Entity<WettableComponent> ent, ref WetnessChangedEvent args)
    {
        if (_container.TryGetContainingContainer(ent.Owner, out var container))
            Recompute(container.Owner);
    }

    private void OnStainChanged(Entity<StainableComponent> ent, ref StainChangedEvent args)
    {
        if (_container.TryGetContainingContainer(ent.Owner, out var container))
            Recompute(container.Owner);
    }

    private void Recompute(EntityUid mob)
    {
        if (!HasComp<MoodComponent>(mob) ||
            !_inventory.TryGetContainerSlotEnumerator(mob, out var enumerator, SlotFlags.WITHOUT_POCKET))
        {
            return;
        }

        var wet = 0;
        var dirty = 0;
        while (enumerator.NextItem(out var item))
        {
            if (_wetness.IsWet(item))
                wet++;
            if (_stains.HasStain(item))
                dirty++;
        }

        UpdateEffect(mob, WetMoodId, wet);
        UpdateEffect(mob, DirtyMoodId, dirty);
    }

    private void UpdateEffect(EntityUid mob, string effectId, int count)
    {
        if (count > 0)
            RaiseLocalEvent(mob, new MoodEffectEvent(effectId, count));
        else
            RaiseLocalEvent(mob, new MoodRemoveEffectEvent(effectId));
    }

    private void TryGrantFreshLaundry(EntityUid mob, EntityUid item, SlotFlags slot)
    {
        if ((slot & SlotFlags.INNERCLOTHING) == 0 ||
            !TryComp<FreshLaundryComponent>(item, out var fresh) ||
            _timing.CurTime >= fresh.Expiry)
        {
            return;
        }

        RaiseLocalEvent(mob, new MoodEffectEvent(FreshLaundryMoodId));
    }
}
