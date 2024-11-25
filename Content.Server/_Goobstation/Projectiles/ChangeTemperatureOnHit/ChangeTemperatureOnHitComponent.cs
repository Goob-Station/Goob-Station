namespace Content.Server._Goobstation.Projectiles.ChangeTemperatureOnHit;

/// <summary>
/// Changes the temperature of an entity when hit by a projectile
/// </summary>
[RegisterComponent]
public sealed partial class ChangeTemperatureOnHitComponent : Component
{
    /// <summary>
    /// The amount to change the temperature by
    /// </summary>
    [DataField]
    public float Delta;
}
