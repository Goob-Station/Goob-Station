// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Client.Clothing.Components;
using Content.Goobstation.Common.Clothing;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;

namespace Content.Goobstation.Client.Clothing.EntitySystems;

public sealed class HideClothingLayerClothingSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InventoryComponent, CheckClothingSlotHiddenEvent>(OnCheck);

        SubscribeLocalEvent<HideClothingLayerClothingComponent, GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<HideClothingLayerClothingComponent, GotUnequippedEvent>(OnUnequip);
    }

    private void OnUnequip(Entity<HideClothingLayerClothingComponent> ent, ref GotUnequippedEvent args)
    {
        ResetInventory(args.Equipee, ent.Comp);
    }

    private void OnEquip(Entity<HideClothingLayerClothingComponent> ent, ref GotEquippedEvent args)
    {
        ResetInventory(args.Equipee, ent.Comp);
    }

    private void ResetInventory(EntityUid equipee, HideClothingLayerClothingComponent component)
    {
        foreach (var slot in component.HiddenSlots)
        {
            if (_inventory.TryGetSlotEntity(equipee, slot, out var uid))
                _item.VisualsChanged(uid.Value);
        }
    }

    private void OnCheck(Entity<InventoryComponent> ent, ref CheckClothingSlotHiddenEvent args)
    {
        var enumerator = _inventory.GetSlotEnumerator((ent.Owner, ent.Comp), SlotFlags.WITHOUT_POCKET);
        while (enumerator.NextItem(out var item))
        {
            if (!TryComp(item, out HideClothingLayerClothingComponent? hide))
                continue;

            if (!hide.HiddenSlots.Contains(args.Slot))
                continue;

            args.Visible = false;
            return;
        }
    }
}
