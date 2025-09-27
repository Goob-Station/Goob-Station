using Content.Shared.Damage;

namespace Content.Goobstation.Server.Damage;

/// <summary>
/// This stops damage over a set number
/// </summary>
public sealed class PreventDamageSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PreventDamageComponent, BeforeDamageChangedEvent>(OnBeforeDamageChanged);
    }

    private void OnBeforeDamageChanged(Entity<PreventDamageComponent> entity, ref BeforeDamageChangedEvent  arg)
    {
        if(!TryComp<DamageableComponent>(entity.Owner, out var damageable))
           return;

        var currentDaamage = damageable.Damage.GetTotal();
        var newTotaldamage = currentDaamage+arg.Damage.GetTotal();

        if(newTotaldamage > entity.Comp.MaxDamage)
            arg.Cancelled = true;

    }
}
