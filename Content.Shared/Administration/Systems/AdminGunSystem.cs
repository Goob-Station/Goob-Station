// SPDX-FileCopyrightText: 2024 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Administration.Components;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared.Administration.Systems;

public sealed class AdminGunSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<AdminMinigunComponent, GunRefreshModifiersEvent>(OnGunRefreshModifiers);
    }

    private void OnGunRefreshModifiers(Entity<AdminMinigunComponent> ent, ref GunRefreshModifiersEvent args)
    {
        args.FireRate = 15;
    }
}
