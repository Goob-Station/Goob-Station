using Content.Server.Chemistry.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared._Goobstation.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Server._Goobstation.Weapons.Ranged;

/// <summary>
///     System for handling projectiles and altering their properties when fired from a Syringe Gun.
/// </summary>
public sealed class SyringeGunSystem : EntitySystem
{

    public override void Initialize()
    {
        SubscribeLocalEvent<SyringeGunComponent, AmmoShotEvent>(OnFire);
        SubscribeLocalEvent<SyringeGunComponent, AttemptShootEvent>(OnShootAttemot);
    }

    private void OnShootAttemot(Entity<SyringeGunComponent> ent, ref AttemptShootEvent args)
    {
        args.ThrowItems = true;
    }

    private void OnFire(Entity<SyringeGunComponent> gun, ref AmmoShotEvent args)
    {
        foreach (var projectile in args.FiredProjectiles)
        {
            if (TryComp(projectile, out SolutionInjectWhileEmbeddedComponent? whileEmbedded))
            {
                whileEmbedded.Injections = null; // uncap the injection maximum
                whileEmbedded.PierceArmorOverride = gun.Comp.PierceArmor;
                whileEmbedded.AmountMultiplier = gun.Comp.InjectionSpeedMultiplier;
            }
            if (TryComp(projectile, out SolutionInjectOnEmbedComponent? onEmbed))
                onEmbed.PierceArmorOverride = gun.Comp.PierceArmor;
        }
    }

}
