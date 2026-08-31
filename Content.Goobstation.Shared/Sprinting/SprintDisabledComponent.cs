using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Sprinting;

/// <summary>
/// Prevents the entity from sprinting while present..
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SprintDisabledComponent : Component;
