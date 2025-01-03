using Content.Shared.Changeling;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared.Weapons.Ranged.Systems;

public partial class SharedGunSystem
{
    private void InitializeChangeling()
    {
        SubscribeLocalEvent<ChangelingChemicalsAmmoProviderComponent, TakeAmmoEvent>(OnChangelingTakeAmmo);
        SubscribeLocalEvent<ChangelingChemicalsAmmoProviderComponent, GetAmmoCountEvent>(OnChangelingAmmoCount);
    }

    private void OnChangelingAmmoCount(Entity<ChangelingChemicalsAmmoProviderComponent> ent, ref GetAmmoCountEvent args)
    {
        var (uid, component) = ent;

        var parent = Transform(uid).ParentUid;

        if (!TryComp(parent, out ChangelingComponent? ling))
            return;

        if (component.FireCost == 0)
        {
            args.Capacity = int.MaxValue;
            args.Count = int.MaxValue;
            return;
        }

        args.Capacity = (int) (ling.MaxChemicals / component.FireCost);
        args.Count = (int) (ling.Chemicals / component.FireCost);
    }

    private void OnChangelingTakeAmmo(Entity<ChangelingChemicalsAmmoProviderComponent> ent, ref TakeAmmoEvent args)
    {
        var (uid, component) = ent;

        var parent = Transform(uid).ParentUid;

        if (!TryComp(parent, out ChangelingComponent? ling))
            return;

        for (var i = 0; i < args.Shots; i++)
        {
            if (ling.Chemicals < component.FireCost)
                return;

            ling.Chemicals -= component.FireCost;

            var shot = Spawn(component.Proto, args.Coordinates);
            args.Ammo.Add((shot, EnsureShootable(shot)));
        }

        Dirty(parent, ling);
    }
}
