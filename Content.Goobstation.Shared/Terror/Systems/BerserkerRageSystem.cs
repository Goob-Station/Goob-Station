using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// This system is used to increase the damage of an entity based on how much damage it has taken. The damage goes back down once the entity is healed.
/// </summary>
public sealed class BerserkerRageSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BerserkerRageComponent, GetUserMeleeDamageEvent>(OnGetUserDamage);
    }

    private float GetHealth(EntityUid ent)
    {
        if (!TryComp<DamageableComponent>(ent, out var damageable)) return 1f;

        if (!TryComp<MobThresholdsComponent>(ent, out var thresholds)) return 1f;

        float maxDamage = 0f;

        foreach (var (damage, state) in thresholds.Thresholds)
        {
            if (state == MobState.Dead)
            {
                maxDamage = (float) damage;
                break;
            }
        }

        if (maxDamage <= 0f) return 1f;

        var totalDamage = (float) damageable.TotalDamage;
        return Math.Clamp(1f - (totalDamage / maxDamage), 0f, 1f);
    }

    private void OnGetUserDamage(Entity<BerserkerRageComponent> ent, ref GetUserMeleeDamageEvent args)
    {
        var comp = ent.Comp;
        var health = GetHealth(ent.Owner);

        var multiplier = MathHelper.Lerp(comp.MaxMultiplier, comp.MinMultiplier, health);

        foreach (var (type, oldValue) in args.Damage.DamageDict)
        {
            args.Damage.DamageDict[type] = oldValue * multiplier;
        }
    }
}
