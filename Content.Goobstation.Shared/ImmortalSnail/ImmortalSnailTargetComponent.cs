using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.ImmortalSnail;

/// <summary>
/// Marker component added to immortal snail targets so we can track their deletion.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ImmortalSnailTargetComponent : Component
{
}
