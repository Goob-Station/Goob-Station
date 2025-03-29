using Content.Shared.Mech.Components;

namespace Content.Shared._Goobstation.Mech.Components.Malfunctions;

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
