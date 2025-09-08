namespace Content.Goobstation.Common.Traits;

/// <summary>
/// Component indicating that a mob is cold-blooded. When this is present on a mob,
/// it will cause ThermalRegulatorSystem to ignore it and will tweak its temperature
/// damage thresholds by the values in ColdBloodedSystem
/// </summary>
[RegisterComponent]
public sealed partial class ColdBloodedComponent : Component
{
    /// <summary>
    /// How much the cold damage threshold is increased when the cold-blooded trait is applied.
    /// </summary>
    [DataField]
    public float ColdThresholdIncrease;

    /// <summary>
    /// How much the heat damage threshold is increased when the cold-blooded trait is applied.
    /// </summary>
    [DataField]
    public float HeatThresholdIncrease;

    /// <summary>
    /// How much is the ability for heat to transfer from the atmosphere to you increase.
    /// </summary>
    [DataField]
    public float AtmosTransferMultiplier;
}
