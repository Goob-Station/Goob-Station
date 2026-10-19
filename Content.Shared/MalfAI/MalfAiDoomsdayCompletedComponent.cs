using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Marker component indicating a Malf AI has completed doomsday.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfAiDoomsdayCompletedComponent : Component
{
    // No fields needed; presence of this component means doomsday is complete.
}

