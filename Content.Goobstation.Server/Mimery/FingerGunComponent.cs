namespace Content.Goobstation.Server.Mimery;

[RegisterComponent]
public sealed partial class FingerGunComponent : Component
{
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool OnHand = true;

    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool FingerGunExists;

    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid FingerGun;

    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public float CurrentCharge;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public float LastCharge;

    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public float ChargeRate = 25;

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan LastChargeTime;

}
