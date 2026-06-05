// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.InternalResources.EntitySystems;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Goobstation.Shared.Changeling.Systems;

public sealed class ChangelingGunSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _guns = default!;
    [Dependency] private readonly SharedInternalResourcesSystem _resource = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ChangelingChemicalsAmmoProviderComponent, TakeAmmoEvent>(OnChangelingTakeAmmo);
        SubscribeLocalEvent<ChangelingChemicalsAmmoProviderComponent, GetAmmoCountEvent>(OnChangelingAmmoCount);
    }

    private void OnChangelingAmmoCount(Entity<ChangelingChemicalsAmmoProviderComponent> ent, ref GetAmmoCountEvent args)
    {
        var (uid, component) = ent;

        var parent = Transform(uid).ParentUid;

        if (!TryComp<ChangelingChemicalComponent>(parent, out var chemComp)
            || chemComp.ResourceData == null)
            return;

        if (component.FireCost == 0)
        {
            args.Capacity = int.MaxValue;
            args.Count = int.MaxValue;
            return;
        }

        args.Capacity = (int) (chemComp.ResourceData.MaxAmount / component.FireCost);
        args.Count = (int) (chemComp.ResourceData.CurrentAmount / component.FireCost);
    }

    private void OnChangelingTakeAmmo(Entity<ChangelingChemicalsAmmoProviderComponent> ent, ref TakeAmmoEvent args)
    {
        var (uid, component) = ent;

        var parent = Transform(uid).ParentUid;

        if (!TryComp<ChangelingChemicalComponent>(parent, out var chemComp)
            || chemComp.ResourceData == null)
            return;

        for (var i = 0; i < args.Shots; i++)
        {
            if (chemComp.ResourceData.CurrentAmount < component.FireCost)
                return;

            _resource.TryUpdateResourcesAmount(parent, chemComp.ResourceData, -component.FireCost);

            var shot = Spawn(component.Proto, args.Coordinates);
            args.Ammo.Add((shot, _guns.EnsureShootable(shot)));
        }
    }
}
