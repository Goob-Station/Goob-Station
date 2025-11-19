using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Damage.Components;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Controls <see cref="DamageOnAttackedComponent"/> damage for <see cref="ThornsEnchantComponent"/>.
/// </summary>
public sealed class ThornsEnchantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThornsEnchantComponent, EnchantAddedEvent>(OnAdded);
        SubscribeLocalEvent<ThornsEnchantComponent, EnchantUpgradedEvent>(OnUpgraded);
    }

    private void OnAdded(Entity<ThornsEnchantComponent> ent, ref EnchantAddedEvent args)
    {
        ScaleDamage(ent, (float) args.Level);
    }

    private void OnUpgraded(Entity<ThornsEnchantComponent> ent, ref EnchantUpgradedEvent args)
    {
        ScaleDamage(ent, (float) args.Level / (float) args.OldLevel);
    }

    private void ScaleDamage(EntityUid uid, float factor)
    {
        var comp = Comp<DamageOnAttackedComponent>(uid);
        comp.Damage *= factor;
        Dirty(uid, comp);
    }
}
