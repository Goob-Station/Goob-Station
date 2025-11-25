using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;
using System.Linq;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// This system is used to increase the damage of an entity based on how much damage it has taken. The damage goes back down once the entity is healed.
/// </summary>
public sealed class BerserkerRageSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<BerserkerRageComponent, GetUserMeleeDamageEvent>(OnGetUserDamage);
    }

    private float GetHealth(EntityUid ent)
    {
        if (!TryComp<DamageableComponent>(ent, out var damageable))
            return 1f;

        var damage = (float) damageable.TotalDamage;
        const float maxDamage = 100f;

        return Math.Clamp(1f - (damage / maxDamage), 0f, 1f);
    }

    private void OnGetUserDamage(Entity<BerserkerRageComponent> ent, ref GetUserMeleeDamageEvent args)
    {
        var comp = ent.Comp;

        var health = GetHealth(ent.Owner);

        // MinMultiplier at high health, MaxMultiplier at low health
        var multiplier = MathHelper.Lerp(comp.MaxMultiplier, comp.MinMultiplier, health);

        foreach (var type in args.Damage.DamageDict.Keys.ToArray())
        {
            var oldValue = args.Damage.DamageDict[type];
            var newValue = oldValue * multiplier;
            args.Damage.DamageDict[type] = newValue;
        }
    }
}
