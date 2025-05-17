// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Goobstation.Server.Weapons.BatterySlotRequiresItemToggle;

public sealed class BatterySlotRequiresToggleSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatterySlotRequiresToggleComponent, ItemToggledEvent>(OnToggle);
    }

    private void OnToggle(Entity<BatterySlotRequiresToggleComponent> ent, ref ItemToggledEvent args)
    {
        if (!TryComp<ItemSlotsComponent>(ent, out var itemslots)
            || !_itemSlotsSystem.TryGetSlot(ent, ent.Comp.ItemSlot, out var slot, itemslots))
            return;

        _itemSlotsSystem.SetLock(ent, slot, args.Activated ^ ent.Comp.Inverted, itemslots);
    }
}
