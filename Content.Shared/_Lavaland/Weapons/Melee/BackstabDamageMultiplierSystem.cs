using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Lavaland.Weapons.Melee.Backstab;

public sealed class BackstabDamageMultiplierSystem : EntitySystem
{
    [Dependency] protected readonly DamageableSystem _damageable = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<BackstabDamageMultiplierComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<BackstabDamageMultiplierComponent> ent, ref MeleeHitEvent args)
    {
        foreach (var damaged in args.HitEntities)
        {
            var degrees = Transform(damaged).LocalRotation.Degrees - Transform(args.User).LocalRotation.Degrees;
            if (degrees >= 300 || degrees <= 60 && degrees >= -30) // John Shitcode
            {
                _damageable.TryChangeDamage(damaged, ent.Comp.BonusDamage, origin:args.User);
            }
        }
    }
}
