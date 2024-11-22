namespace Content.Server._Goobstation.Projectiles.ChangeTemperatureOnHit;

[RegisterComponent]
public sealed partial class ChangeTemperatureOnHitComponent : Component
{
    [DataField]
    public float Delta;
}
