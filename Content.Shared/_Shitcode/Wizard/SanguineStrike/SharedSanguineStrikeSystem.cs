using System.Linq;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.SanguineStrike;

public abstract class SharedSanguineStrikeSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SanguineStrikeComponent, MeleeHitEvent>(OnHit);
        SubscribeLocalEvent<SanguineStrikeComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<SanguineStrikeComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("sanguine-strike-examine"));
    }

    private void OnHit(Entity<SanguineStrikeComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        if (args.HitEntities.Contains(args.User))
            return;

        var mobStateQuery = GetEntityQuery<MobStateComponent>();
        var hitMobs = args.HitEntities
            .Where(x => mobStateQuery.TryComp(x, out var mobState) && mobState.CurrentState != MobState.Dead)
            .ToList();
        if (hitMobs.Count == 0)
            return;

        var (uid, comp) = ent;

        var damageWithoutStructural = args.BaseDamage;
        damageWithoutStructural.DamageDict.Remove("Structural");
        var damage = damageWithoutStructural * comp.DamageMultiplier;
        var totalBaseDamage = damageWithoutStructural.GetTotal();
        var totalDamage = totalBaseDamage * comp.DamageMultiplier;
        if (totalDamage > 0f && totalDamage > comp.MaxDamageModifier)
        {
            damage *= comp.MaxDamageModifier / totalDamage;
            damage += damageWithoutStructural;
        }

        var newTotalDamage = damage.GetTotal();
        if (newTotalDamage > totalBaseDamage)
            args.BonusDamage += damage - damageWithoutStructural;
        args.HitSoundOverride = comp.HitSound;

        LifeSteal(args.User, newTotalDamage);

        Hit(uid, comp, args.User, hitMobs);
    }

    protected virtual void Hit(EntityUid uid,
        SanguineStrikeComponent component,
        EntityUid user,
        IReadOnlyList<EntityUid> hitEntities)
    {
    }

    public virtual void BloodSteal(EntityUid user,
        IReadOnlyList<EntityUid> hitEntities,
        FixedPoint2 bloodStealAmount,
        EntityCoordinates? bloodSpillCoordinates)
    {
    }

    public virtual void ParticleEffects(EntityUid user, IReadOnlyList<EntityUid> targets, EntProtoId particle)
    {
    }

    public void LifeSteal(EntityUid uid, FixedPoint2 amount, DamageableComponent? damageable = null)
    {
        if (!Resolve(uid, ref damageable, false))
            return;

        var totalUserDamage = damageable.TotalDamage;
        if (totalUserDamage <= FixedPoint2.Zero)
            return;

        DamageSpecifier toHeal;
        if (amount < totalUserDamage)
            toHeal = damageable.Damage * amount / totalUserDamage;
        else
            toHeal = damageable.Damage;

        _damageable.TryChangeDamage(uid, -toHeal, true, false, damageable, null, false, targetPart: TargetBodyPart.All);
    }
}
