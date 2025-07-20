using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared.Humanoid;
using Content.Shared.Roles;
using Content.Shared.Station;
using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Clothing.Systems;
public sealed class LoadoutSpeciesSystem : EntitySystem
{
    [Dependency] private readonly SharedStationSpawningSystem _station = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoadoutSpeciesComponent, StartingGearEquippedEvent>(OnGearEquipped);
    }

    private void OnGearEquipped(EntityUid uid, LoadoutSpeciesComponent component, ref StartingGearEquippedEvent args)
    {
        if (component.Overridden ||
            !TryComp<HumanoidAppearanceComponent>(uid, out var appearance))
            return;

        // StartingGear override
        if (component.SpeciesStartingGearOverride.TryGetValue(appearance.Species, out var overrideGear) &&
            TryComp<InventoryComponent>(uid, out var inv))
        {
            foreach (var slotDef in inv.Slots)
            {
                if (_inventory.TryGetSlotEntity(uid, slotDef.Name, out var oldItem))
                    EntityManager.DeleteEntity(oldItem.Value);
            }
            Dirty(uid, component);
            component.Overridden = true;
            _station.EquipStartingGear(uid, overrideGear);
        }

        // Slot-specific override
        if (!component.SpeciesSlotOverride.TryGetValue(appearance.Species, out var slotOverrides))
            return;

        foreach (var (slot, protoId) in slotOverrides)
        {
            if (_inventory.TryGetSlotEntity(uid, slot, out var oldItem))
            {
                EntityManager.DeleteEntity(oldItem.Value);
            }
            _inventory.SpawnItemInSlot(uid, slot, protoId);
        }

    }
}
