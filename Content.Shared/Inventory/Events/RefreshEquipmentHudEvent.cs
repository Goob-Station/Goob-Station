// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Inventory.Events;

[ByRefEvent]
public record struct RefreshEquipmentHudEvent<T>(SlotFlags TargetSlots, bool WorksInHands = false) : IInventoryRelayEvent // Goob edit
    where T : IComponent
{
    public bool WorksInHands = WorksInHands; // Goobstation
    public SlotFlags TargetSlots { get; } = TargetSlots;
    public bool Active = false;
    public List<T> Components = new();
}
