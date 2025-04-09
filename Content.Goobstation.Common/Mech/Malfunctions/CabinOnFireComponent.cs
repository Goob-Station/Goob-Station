namespace Content.Goobstation.Common.Mech.Malfunctions;

[RegisterComponent]
public sealed partial class CabinOnFireComponent : Component
{
}

/// <summary>
/// Raises when CabinOnFireEvent randomly picked in MechSystem.
/// </summary>
public sealed partial class CabinOnFireEvent : BaseMalfunctionEvent
{
}
