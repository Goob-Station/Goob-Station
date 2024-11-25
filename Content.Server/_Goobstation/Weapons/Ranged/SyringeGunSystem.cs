using Content.Server.Chemistry.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared._Goobstation.Weapons.Ranged;
namespace Content.Server._Goobstation.Weapons.Ranged;

/// <summary>
///     System for handling projectiles and altering their properties when fired from a Syringe Gun.
/// </summary>
public sealed class SyringeGunSystem : EntitySystem
{

    public override void Initialize()
    {
        SubscribeLocalEvent<SyringeGunComponent, AmmoShotEvent>(OnFire);
    }

    private void OnFire(Entity<SyringeGunComponent> gun, ref AmmoShotEvent args)
    {
        foreach (var projectile in args.FiredProjectiles)
            if (TryComp(projectile, out SolutionInjectOnEmbedComponent? inject))
            {
                inject.Shot = true;
                inject.PierceArmor = gun.Comp.PierceArmor;
            }
    }

}
