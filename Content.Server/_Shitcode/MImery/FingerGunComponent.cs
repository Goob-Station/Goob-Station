namespace Content.Server._Shitcode.Mimery;

[RegisterComponent]
public sealed partial class FingerGunComponent : Component
{

    public bool OnHand = true;

    public bool FingerGunExists;

    public EntityUid FingerGun;

    public float CurrentCharge;

    public float LastCharge;

    public float ChargeRate = 25;

    public TimeSpan LastChargeTime;

}
