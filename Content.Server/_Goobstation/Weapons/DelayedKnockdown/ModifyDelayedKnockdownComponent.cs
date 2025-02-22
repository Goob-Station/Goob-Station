namespace Content.Server._Goobstation.Weapons.DelayedKnockdown;

[RegisterComponent]
public sealed partial class ModifyDelayedKnockdownComponent : Component
{
    [DataField]
    public bool Cancel;

    [DataField]
    public float DelayDelta;

    [DataField]
    public float KnockdownTimeDelta;
}
