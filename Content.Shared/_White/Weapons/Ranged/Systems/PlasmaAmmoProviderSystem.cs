using Content.Shared._White.Weapons.Ranged.Components;
using Content.Shared._White.Xenomorphs.Plasma;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared._White.Weapons.Ranged.Systems;

public sealed class PlasmaAmmoProviderSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedPlasmaSystem _plasma = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlasmaAmmoProviderComponent, TakeAmmoEvent>(OnTakeAmmo);
        SubscribeLocalEvent<PlasmaAmmoProviderComponent, GetAmmoCountEvent>(OnGetAmmoCount);
    }

    private void OnTakeAmmo(EntityUid uid, PlasmaAmmoProviderComponent component, ref TakeAmmoEvent args)
    {
        if (!TryComp<PlasmaVesselComponent>(uid, out var plasmaVessel))
            return;

        for (var i = 0; i < args.Shots; i++)
        {
            if (!_plasma.ChangePlasmaAmount(uid, -component.FireCost, plasmaVessel))
                return;

            var shot = Spawn(component.Proto, args.Coordinates);
            args.Ammo.Add((shot, _gun.EnsureShootable(shot)));
        }
    }

    private void OnGetAmmoCount(EntityUid uid, PlasmaAmmoProviderComponent component, ref GetAmmoCountEvent args)
    {
        if (!TryComp<PlasmaVesselComponent>(uid, out var plasmaVessel))
            return;

        args.Capacity = (int) (plasmaVessel.MaxPlasma / component.FireCost);
        args.Count = (int) (plasmaVessel.Plasma / component.FireCost);
    }
}
