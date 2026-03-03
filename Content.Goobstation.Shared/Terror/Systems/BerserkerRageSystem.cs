using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// This system is used to increase the damage of an entity based on how much damage it has taken. The damage goes back down once the entity is healed.
/// </summary>
public sealed class BerserkerRageSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<BerserkerRageComponent, GetUserMeleeDamageEvent>(OnGetUserDamage);
    }

    private float GetHealth(EntityUid ent)
    {
        if (!TryComp<DamageableComponent>(ent, out var damageable))
            return 1f;

        if (!TryComp<MobThresholdsComponent>(ent, out var thresholds))
            return 1f;

        // Find the damage value that corresponds to dead state which is basically max health
        float maxDamage = 0f;

        foreach (var (damage, state) in thresholds.Thresholds)
        {
            if (state == MobState.Dead)
            {
                maxDamage = (float) damage;
                break;
            }
        }

        if (maxDamage <= 0f)
            return 1f;

        var totalDamage = (float) damageable.TotalDamage;
        return Math.Clamp(1f - (totalDamage / maxDamage), 0f, 1f);
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
