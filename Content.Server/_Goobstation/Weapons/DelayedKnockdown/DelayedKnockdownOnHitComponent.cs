namespace Content.Server._Goobstation.Weapons.DelayedKnockdown;

[RegisterComponent]
public sealed partial class DelayedKnockdownOnHitComponent : Component
{
    [DataField]
    public float Delay = 2f;

    [DataField]
    public float KnockdownTime = 4f;

    [DataField]
    public bool Refresh = true;

    [DataField]
    public bool ApplyOnHeavyAttack;

    [DataField]
    public string UseDelay = "default";
}
