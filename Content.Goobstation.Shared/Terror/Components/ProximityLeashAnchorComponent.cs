using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Marks an entity as anchor for ProximityLeashComponent.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ProximityLeashAnchorComponent : Component
{
    /// <summary>
    /// Must match the LeashGroup on the leashed entity's ProximityLeashComponent.
    /// </summary>
    [DataField]
    public string LeashGroup = "default";
}
