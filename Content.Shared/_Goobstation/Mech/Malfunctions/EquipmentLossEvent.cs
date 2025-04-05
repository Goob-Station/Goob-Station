using Content.Shared.Mech.Components;

namespace Content.Shared._Goobstation.Components.Malfunctions;

/// <summary>
/// Raises when EquipmentLossEvent randomly picked in MechSystem.
/// </summary>
public sealed partial class EquipmentLossEvent : BaseMalfunctionEvent
{
    [DataField]
    public float Range = 3f;
    public EquipmentLossEvent(float range)
    {
        Range = range;
    }
}
