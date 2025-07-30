using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Overlays;

namespace Content.Pirate.Server.Overlays;

/// <summary>
/// System to handle psionics record icons inventory relay events
/// This replaces the functionality that was in Content.Shared for psionics
/// Since Content.Shared (Core Module) cannot reference Content.Pirate modules,
/// we handle the psionics inventory relay functionality here instead.
/// </summary>
public sealed class PsionicsInventoryRelaySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        // Subscribe to the inventory relay event for psionics record icons
        // This provides the same functionality that was attempted in Content.Shared
        SubscribeLocalEvent<InventoryComponent, RefreshEquipmentHudEvent<ShowPsionicsRecordIconsComponent>>(OnRefreshEquipmentHud);
    }

    private void OnRefreshEquipmentHud(EntityUid uid, InventoryComponent component, RefreshEquipmentHudEvent<ShowPsionicsRecordIconsComponent> args)
    {
        // Handle the relay event for psionics record icons
        // This mimics the RefRelayInventoryEvent behavior from Content.Shared

        // Relay the event to all equipped items
        if (!TryComp<InventoryComponent>(uid, out var inventory))
            return;

        foreach (var slot in inventory.Containers)
        {
            foreach (var item in slot.ContainedEntities)
            {
                RaiseLocalEvent(item, ref args);
            }
        }
    }
}
