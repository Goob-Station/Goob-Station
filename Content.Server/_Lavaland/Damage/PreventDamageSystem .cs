using Content.Server._Lavaland.Damage.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Server.Atmos.EntitySystems;

namespace Content.Server._Lavaland.Damage;

/// <summary>
/// This stops a mob from taking damage if that damage leads to its death
/// </summary>
public sealed class PreventDamageSystem  : EntitySystem
{

    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PreventDamageComponent, BeforeDamageChangedEvent>(OnBeforeDamageChanged);
    }

    private void OnBeforeDamageChanged(Entity<PreventDamageComponent> entity, ref BeforeDamageChangedEvent  arg)
    {
        if(!TryComp<DamageableComponent>(entity.Owner, out var damageable))
           return;

        if (!_mobThreshold.TryGetThresholdForState(entity.Owner,MobState.Critical, out var critThreshold))
            return; // systems don't work if there is no critical threshold, this is to prevent immortality

        if (!_mobThreshold.TryGetThresholdForState(entity.Owner,MobState.Dead, out var deadThreshold))
            return; // no death threshold

        if (entity.Comp.LockedToLavaland && !IsOnLavaland(entity))
            return; //Lavaland lock

        if (deadThreshold - critThreshold < entity.Comp.DifferensMinimum) // prevents misuse if the critical and death trigger is the same value
            return;

        var currentDamage = damageable.Damage.GetTotal();
        var newTotalDamage = currentDamage+arg.Damage.GetTotal();

        if(newTotalDamage > deadThreshold)
            arg.Cancelled = true;
    }

    private  bool IsOnLavaland(Entity<PreventDamageComponent> entity)
    {

        var pressure = _atmos.GetTileMixture((entity.Owner, Transform(entity)))?.Pressure ?? 0f;

        if ((pressure >= entity.Comp.LowerBound &&
             pressure <= entity.Comp.UpperBound))
            return true;

        return false;

    }
}
