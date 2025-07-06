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
