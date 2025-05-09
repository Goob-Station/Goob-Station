// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Mobs;

namespace Content.Goidastation.Shared.Clothing;

public record struct ClothingAutoInjectRelayedEvent(EntityUid Target, MobState NewState) : IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}

