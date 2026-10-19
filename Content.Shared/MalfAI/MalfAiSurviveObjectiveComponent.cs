using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Marker component for Malf AI survive objectives.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfAiSurviveObjectiveComponent : Component
{
    // No fields needed; progress is determined by system logic.
}
