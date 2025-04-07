// SPDX-FileCopyrightText: 2025 Milon <milonpl.git@proton.me>
// SPDX-FileCopyrightText: 2024 PrPleGoo <PrPleGoo@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared.Inventory.Events;

[ByRefEvent]
public record struct RefreshEquipmentHudEvent<T>(SlotFlags TargetSlots) : IInventoryRelayEvent
    where T : IComponent
{
    public SlotFlags TargetSlots { get; } = TargetSlots;
    public bool Active = false;
    public List<T> Components = new();
}