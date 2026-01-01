// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Interaction;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Shared.Weapons.Ranged.Systems;

/// <summary>
/// Recharges ammo whenever the gun is cycled.
/// </summary>
public sealed class RechargeCycleAmmoSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RechargeCycleAmmoComponent, ActivateInWorldEvent>(OnRechargeCycled);
    }

    private void OnRechargeCycled(Entity<RechargeCycleAmmoComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex)
            return;

        if (!TryComp<BasicEntityAmmoProviderComponent>(ent, out var basic) || args.Handled)
            return;

        if (basic.Count >= basic.Capacity || basic.Count == null)
            return;

        _gun.UpdateBasicEntityAmmoCount((ent, basic), basic.Count.Value + 1);
        Dirty(ent, basic);
        args.Handled = true;
    }
}