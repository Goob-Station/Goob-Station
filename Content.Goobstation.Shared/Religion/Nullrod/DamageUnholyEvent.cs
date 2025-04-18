using Content.Shared.Damage;

namespace Content.Goobstation.Shared.Religion.Nullrod;

public sealed class DamageUnholyEvent : EntityEventArgs
{
    public readonly DamageSpecifier OriginalDamage;
    public readonly EntityUid Target;
    public DamageSpecifier Damage;
    public bool Handled = false;
    public EntityUid? Origin;

    public DamageUnholyEvent(EntityUid target,
        DamageSpecifier damage,
        EntityUid? origin = null)
    {
        Target = target;
        OriginalDamage = damage;
        Damage = damage;
        Origin = origin;
    }
}
