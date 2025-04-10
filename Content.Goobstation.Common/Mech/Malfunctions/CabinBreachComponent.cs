namespace Content.Goobstation.Common.Mech.Malfunctions;

[RegisterComponent]
public sealed partial class CabinBreachComponent : Component
{
}

/// <summary>
/// Raises when CabinBreachEvent randomly picked in MechSystem.
/// </summary>
public sealed partial class CabinBreachEvent : BaseMalfunctionEvent
{
}
