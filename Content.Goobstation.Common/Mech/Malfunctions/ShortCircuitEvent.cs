namespace Content.Goobstation.Common.Mech.Malfunctions;

/// <summary>
/// Raises when ShortCircuitEvent randomly picked in MechSystem.
/// </summary>
public sealed partial class ShortCircuitEvent : BaseMalfunctionEvent
{
    [DataField]
    public float EnergyLoss = 30;

    public ShortCircuitEvent(float energyLoss)
    {
        EnergyLoss = energyLoss;
    }
}
