using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Weapons.Melee.Events;
using JetBrains.Annotations;

namespace Content.Goobstation.Shared.Slasher.Systems;

public sealed partial class SharedSlasherSoulStealSystem : EntitySystem
{
    [PublicAPI]
    public void SetBonus(EntityUid machete, DamageSpecifier bonus)
    {
        var comp = EnsureComp<SlasherSoulStealMacheteBonusComponent>(machete);
        comp.DamageBonus = new DamageSpecifier(bonus);
        Dirty(machete, comp);
        UpdateThrowDamage((machete, comp));
    }

    [SubscribeLocalEvent]
    private void OnAfterHandleState(Entity<SlasherSoulStealMacheteBonusComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateThrowDamage(ent);
    }

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<SlasherSoulStealMacheteBonusComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.DamageBonus = new DamageSpecifier();
        UpdateThrowDamage(ent);
    }

    [SubscribeLocalEvent]
    private void OnGetMeleeDamage(Entity<SlasherSoulStealMacheteBonusComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (ent.Comp.DamageBonus.Empty)
            return;

        args.Damage += ent.Comp.DamageBonus;
    }

    private void UpdateThrowDamage(Entity<SlasherSoulStealMacheteBonusComponent> ent)
    {
        if (!TryComp<DamageOtherOnHitComponent>(ent.Owner, out var throwDamage))
            return;

        ent.Comp.BaseThrowDamage ??= new DamageSpecifier(throwDamage.Damage);

        var damage = throwDamage.Damage;
        damage.DamageDict.Clear();
        foreach (var (type, value) in (ent.Comp.BaseThrowDamage + ent.Comp.DamageBonus).DamageDict)
            damage.DamageDict[type] = value;
    }
}
