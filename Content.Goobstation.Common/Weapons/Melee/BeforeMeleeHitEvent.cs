namespace Content.Goobstation.Common.Melee;

[ByRefEvent]
public sealed class BeforeMeleeHitEvent : HandledEntityEventArgs
{
    public readonly EntityUid Weapon;
    public readonly EntityUid User;
    public readonly float TotalDamage; // goida (i love circular dependencies)
    public readonly bool Heavy;

    public BeforeMeleeHitEvent(EntityUid weapon, EntityUid user, float damage, bool isHeavy = false)
    {
        Weapon = weapon;
        User = user;
        TotalDamage = damage;
        Heavy = isHeavy;
    }
}
