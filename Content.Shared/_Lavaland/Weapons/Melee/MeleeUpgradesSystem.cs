using Content.Goobstation.Common.Weapons;
using Content.Shared._Lavaland.Weapons.Melee.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Lavaland.Weapons.Melee;

public abstract class SharedMeleeUpgradesSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeaponUpgradeDamageComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
        SubscribeLocalEvent<WeaponUpgradeRangeComponent, GetLightAttackRangeEvent>(OnGetRange);
        SubscribeLocalEvent<WeaponUpgradeSpeedComponent, GetMeleeAttackRateEvent>(OnGetAttackRate);
    }

    private void OnGetMeleeDamage(Entity<WeaponUpgradeDamageComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (ent.Comp.BonusDamage != null)
            args.Damage += ent.Comp.BonusDamage;
        args.Damage *= ent.Comp.Modifier;
    }

    private void OnGetRange(Entity<WeaponUpgradeRangeComponent> ent, ref GetLightAttackRangeEvent args)
    {
        if (ent.Comp.BonusRange != null)
            args.Range += ent.Comp.BonusRange.Value;
        if (ent.Comp.RangeMultiplier != null)
            args.Range *= ent.Comp.RangeMultiplier.Value;
    }

    private void OnGetAttackRate(Entity<WeaponUpgradeSpeedComponent> ent, ref GetMeleeAttackRateEvent args)
    {
        if (ent.Comp.BonusAttackRate != null)
            args.Rate += ent.Comp.BonusAttackRate.Value;
        if (ent.Comp.AttackRateMultiplier != null)
            args.Multipliers *= ent.Comp.AttackRateMultiplier.Value;
    }
}
