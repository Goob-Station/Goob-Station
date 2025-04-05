using Content.Shared.Actions;
using Content.Shared.FixedPoint;
using Content.Shared.Mech.Components;

namespace Content.Shared._Goobstation.Mech.Components.Malfunctions;
/// <summary>
/// Raises when ShortCircuitEvent randomly picked in MechSystem.
/// </summary>
public sealed partial class ShortCircuitEvent : BaseMalfunctionEvent
{
    [DataField]
    public FixedPoint2 EnergyLoss = 30;

    public ShortCircuitEvent(FixedPoint2 energyLoss)
    {
        EnergyLoss = energyLoss;
    }
}
