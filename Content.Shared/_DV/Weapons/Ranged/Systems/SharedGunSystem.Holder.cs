// SPDX-FileCopyrightText: 2025 Avalon <jfbentley1@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    private void InitializeHolders()
    {
        SubscribeLocalEvent<GunComponent, GotEquippedHandEvent>(OnGunEquipped);
        SubscribeLocalEvent<GunComponent, GotUnequippedHandEvent>(OnGunUnequipped);
    }

    private void OnGunEquipped(Entity<GunComponent> ent, ref GotEquippedHandEvent args)
    {
        ent.Comp.Holder = args.User;
        RefreshModifiers((ent, ent));
    }

    private void OnGunUnequipped(Entity<GunComponent> ent, ref GotUnequippedHandEvent args)
    {
        ent.Comp.Holder = null;
        RefreshModifiers((ent, ent));
    }
}
