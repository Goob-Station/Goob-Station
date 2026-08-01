// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.Mutate;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{

    [Dependency] private readonly SharedGunSystem _gun = default!;

    protected virtual void InitializeBasicHitScan()
    {
        SubscribeLocalEvent<BasicHitscanAmmoProviderComponent, TakeAmmoEvent>(OnBasicHitscanTakeAmmo);
        SubscribeLocalEvent<BasicHitscanAmmoProviderComponent, GetAmmoCountEvent>(OnBasicHitscanAmmoCount);
    }

    private void OnBasicHitscanAmmoCount(Entity<BasicHitscanAmmoProviderComponent> ent, ref GetAmmoCountEvent args)
    {
        args.Capacity = int.MaxValue;
        args.Count = int.MaxValue;
    }

    private void OnBasicHitscanTakeAmmo(Entity<BasicHitscanAmmoProviderComponent> ent, ref TakeAmmoEvent args)
    {
        for (var i = 0; i < args.Shots; i++)
        {
            var hitscanEnt = Spawn(ent.Comp.Proto);
            args.Ammo.Add((hitscanEnt, _gun.EnsureShootable(hitscanEnt)));
        }
    }
}
