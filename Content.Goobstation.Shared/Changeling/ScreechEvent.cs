// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Changeling;

public sealed class SoundSuppressionPresenceEvent : EntityEventArgs, IInventoryRelayEvent
{
    public bool SoundProtected = false;
    public SlotFlags TargetSlots => SlotFlags.EARS | SlotFlags.HEAD;
}
