// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Lock;

namespace Content.Shared.Containers.ItemSlots;

public sealed partial class ItemSlotsSystem
{
    private void InitializeLock()
    {
        SubscribeLocalEvent<ItemSlotsLockComponent, MapInitEvent>(OnLockMapInit);
        SubscribeLocalEvent<ItemSlotsLockComponent, LockToggledEvent>(OnLockToggled);
    }

    private void OnLockMapInit(Entity<ItemSlotsLockComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp(ent.Owner, out LockComponent? lockComp))
            return;

        UpdateLocks(ent, lockComp.Locked);
    }

    private void OnLockToggled(Entity<ItemSlotsLockComponent> ent, ref LockToggledEvent args)
    {
        UpdateLocks(ent, args.Locked);
    }

    private void UpdateLocks(Entity<ItemSlotsLockComponent> ent, bool value)
    {
        foreach (var slot in ent.Comp.Slots)
        {
            if (!TryGetSlot(ent.Owner, slot, out var itemSlot))
                continue;

            SetLock(ent.Owner, itemSlot, value);
        }
    }
}