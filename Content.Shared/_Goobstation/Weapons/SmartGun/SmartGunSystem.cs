using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Wieldable.Components;

namespace Content.Shared._Goobstation.Weapons.SmartGun;

public sealed class SmartGunSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmartGunComponent, AmmoShotEvent>(OnShot);
    }

    private void OnShot(Entity<SmartGunComponent> ent, ref AmmoShotEvent args)
    {
        var (uid, comp) = ent;

        if (!TryComp(uid, out GunComponent? gun) || gun.Target == null)
            return;

        if (comp.RequiresWield && !(TryComp(uid, out WieldableComponent? wieldable) && wieldable.Wielded))
            return;

        if (gun.Target == Transform(uid).ParentUid)
            return;

        foreach (var projectile in args.FiredProjectiles)
        {
            if (!TryComp(projectile, out HomingProjectileComponent? homing))
                continue;

            homing.Target = gun.Target.Value;
            Dirty(projectile, homing);
        }
    }
}
