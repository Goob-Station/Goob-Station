namespace Content.Goobstation.Common.Mech.Malfunctions;

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
