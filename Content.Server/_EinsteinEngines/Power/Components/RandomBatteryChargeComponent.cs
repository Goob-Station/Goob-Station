using System.Numerics;

namespace Content.Server._EinsteinEngines.Power.Components;

[RegisterComponent]
public sealed partial class RandomBatteryChargeComponent : Component
{
    /// <summary>
    ///     The minimum and maximum max charge the battery can have.
    /// </summary>
    [DataField]
    public Vector2 BatteryMaxMinMax = new(0.85f, 1.15f);

    /// <summary>
    ///     The minimum and maximum current charge the battery can have.
    /// </summary>
    [DataField]
    public Vector2 BatteryChargeMinMax = new(1f, 1f);

    /// <summary>
    ///     False if the randomized charge of the battery should be a multiple of the preexisting current charge of the battery.
    ///     True if the randomized charge of the battery should be a multiple of the max charge of the battery post max charge randomization.
    /// </summary>
    [DataField]
    public bool BasedOnMaxCharge = true;
}
