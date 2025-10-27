using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;

namespace Content.Goobstation.Server.Damage;

/// <summary>
/// This stops a mob from taking damage if that damge leads to its death
/// </summary>
public sealed class PreventDamageSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly MobThresholdSystem _mobThreshold= default!;
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
            return; // systems dont work if there is no critical threshold, this is to prevent immortality

        if (!_mobThreshold.TryGetThresholdForState(entity.Owner,MobState.Dead, out var deadThreshold))
            return; // no death threshold

        if (deadThreshold - critThreshold < entity.Comp.DifferensMinimum) // prevents misuse if the critical and death trigger is the same value
            return;

        var currentDaamage = damageable.Damage.GetTotal();
        var newTotalDamage = currentDaamage+arg.Damage.GetTotal();

        if(newTotalDamage > deadThreshold)
            arg.Cancelled = true;
    }
}
