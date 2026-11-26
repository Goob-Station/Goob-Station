using Content.Shared.Damage.Components;

namespace Content.Shared.Damage.Systems;

public abstract partial class SharedDamageOtherOnHitSystem
{
    /// <summary>
    /// Set damage when ent is thrown at something because Damage is write restricted.
    /// </summary>
    public void SetDamage(Entity<DamageOtherOnHitComponent> ent, DamageSpecifier damage)
    {
        ent.Comp.Damage = damage;
        Dirty(ent);
    }
}
