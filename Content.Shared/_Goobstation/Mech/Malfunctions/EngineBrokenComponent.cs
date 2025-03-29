
namespace Content.Shared.Mech.Components.Malfunctions;

[RegisterComponent]
public sealed partial class EngineBrokenComponent : Component
{
}

/// <summary>
/// Raises when EngineBrokenEvent randomly picked in MechSystem.
/// </summary>
public sealed partial class EngineBrokenEvent : BaseMalfunctionEvent
{
}
