// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Flashbang;

public sealed class GetFlashbangedEvent(float range) : EntityEventArgs, IInventoryRelayEvent
{
    public float ProtectionRange = range;

    public SlotFlags TargetSlots => SlotFlags.EARS | SlotFlags.HEAD;
}
