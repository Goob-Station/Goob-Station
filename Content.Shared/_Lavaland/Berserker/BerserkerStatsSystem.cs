using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Lavaland.Berserker;

/// <summary>
/// Increases damage/speed of entity the lower its health is.
/// Optionally can also grant components at certain health threshholds.
/// These get reverted if you get healed.
/// </summary>
public sealed class BerserkerStatsSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BerserkerStatsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BerserkerStatsComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<BerserkerStatsComponent, GetUserMeleeDamageEvent>(OnGetUserDamage);
        SubscribeLocalEvent<BerserkerStatsComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
    }

    private void OnMapInit(Entity<BerserkerStatsComponent> ent, ref MapInitEvent args)
    {
        UpdateStats(ent);
    }

    private void OnDamageChanged(Entity<BerserkerStatsComponent> ent, ref DamageChangedEvent args)
    {
        UpdateStats(ent);
    }

    private void OnGetUserDamage(Entity<BerserkerStatsComponent> ent, ref GetUserMeleeDamageEvent args)
    {
        if (!ent.Comp.ScaleDamage)
            return;

        var multiplier = ent.Comp.CurrentDamageMultiplier;

        var types = new List<string>(args.Damage.DamageDict.Keys);

        foreach (var type in types)
        {
            args.Damage.DamageDict[type] *= multiplier;
        }
    }

    private void OnRefreshSpeed(Entity<BerserkerStatsComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!ent.Comp.ScaleSpeed)
            return;

        args.ModifySpeed(ent.Comp.CurrentSpeedMultiplier);
    }

    // turn full health into 1 and dead to 0 so we can actualy use the numbers without it being a pain in the ass.
    private float GetHealthFraction(EntityUid uid)
    {
        if (!TryComp<DamageableComponent>(uid, out var damageable))
            return 1f;

        if (!TryComp<MobThresholdsComponent>(uid, out var thresholds))
            return 1f;

        // figuring out which value is death
        var maxDamage = 0f;

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

    // Larp between min and max multiplier based on lost health, optional exponential curve
    private float CalculateMultiplier(float health, float min, float max, bool exponential, float exponent)
    {
        var lost = 1f - health;

        var t = lost;
        if (exponential)
        {
            t = MathF.Pow(lost, exponent);
        }

        return MathHelper.Lerp(min, max, t);
    }

    private void UpdateStats(Entity<BerserkerStatsComponent> ent)
    {
        var comp = ent.Comp;
        var health = GetHealthFraction(ent.Owner);
        var dirty = false;

        if (comp.ScaleDamage)
        {
            var newDamageMult = CalculateMultiplier(health, comp.DamageMinMultiplier, comp.DamageMaxMultiplier, comp.ExponentialScaling, comp.ScalingExponent);

            if (!MathHelper.CloseTo(newDamageMult, comp.CurrentDamageMultiplier))
            {
                comp.CurrentDamageMultiplier = newDamageMult;
                dirty = true;
            }
        }

        if (comp.ScaleSpeed)
        {
            var newSpeedMult = CalculateMultiplier(health, comp.SpeedMinMultiplier, comp.SpeedMaxMultiplier, comp.ExponentialScaling, comp.ScalingExponent);

            if (!MathHelper.CloseTo(newSpeedMult, comp.CurrentSpeedMultiplier))
            {
                comp.CurrentSpeedMultiplier = newSpeedMult;
                dirty = true;
                _movementSpeed.RefreshMovementSpeedModifiers(ent.Owner);
            }
        }

        if (dirty)
        {
            Dirty(ent);
        }

        UpdateComponentThresholds(ent, health);
    }

    private void UpdateComponentThresholds(Entity<BerserkerStatsComponent> ent, float health)
    {
        var comp = ent.Comp;

        for (var i = 0; i < comp.ComponentThresholds.Count; i++)
        {
            var threshold = comp.ComponentThresholds[i];
            var shouldBeActive = health <= threshold.HealthThreshold;
            var isActive = comp.ActiveThresholds.Contains(i);

            if (shouldBeActive && !isActive)
            {
                EntityManager.AddComponents(ent.Owner, threshold.Components);
                comp.ActiveThresholds.Add(i);
            }
            else if (!shouldBeActive && isActive)
            {
                EntityManager.RemoveComponents(ent.Owner, threshold.Components);
                comp.ActiveThresholds.Remove(i);
            }
        }
    }
}
