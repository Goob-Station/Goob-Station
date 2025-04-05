using Content.Shared.Mech.Components;

namespace Content.Shared._Goobstation.Mech.Components.Malfunctions;

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
