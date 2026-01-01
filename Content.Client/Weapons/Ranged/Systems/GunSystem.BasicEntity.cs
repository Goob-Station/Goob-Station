// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Weapons.Ranged.Components;

namespace Content.Client.Weapons.Ranged.Systems;

public partial class GunSystem
{
    protected override void InitializeBasicEntity()
    {
        base.InitializeBasicEntity();
        SubscribeLocalEvent<BasicEntityAmmoProviderComponent, UpdateAmmoCounterEvent>(OnBasicEntityAmmoCount);
    }

    private void OnBasicEntityAmmoCount(Entity<BasicEntityAmmoProviderComponent> ent, ref UpdateAmmoCounterEvent args)
    {
        if (args.Control is DefaultStatusControl control && ent.Comp.Count != null && ent.Comp.Capacity != null)
        {
            control.Update(ent.Comp.Count.Value, ent.Comp.Capacity.Value);
        }
    }
}