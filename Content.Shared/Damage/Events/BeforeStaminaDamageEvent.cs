// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;

namespace Content.Shared.Damage.Events;

/// <summary>
/// Raised before stamina damage is dealt to allow other systems to cancel or modify it.
/// </summary>
[ByRefEvent]
// Goobstation change, added Source param.
// DV change FromMelee
public record struct BeforeStaminaDamageEvent(float Value, EntityUid? Source = null, bool Cancelled = false, bool FromMelee = false) : IInventoryRelayEvent
{
    SlotFlags IInventoryRelayEvent.TargetSlots =>  ~SlotFlags.POCKET;
}
