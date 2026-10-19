using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

public enum MalfAiSabotageType
{
    Doomsday,
    Assassinate,
    Protect
}

/// <summary>
/// Component for Malf AI sabotage objectives (doomsday, assassinate, protect).
/// Uses standard TargetObjectiveComponent for target tracking.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfAiSabotageObjectiveComponent : Component
{
    /// <summary>
    /// Defaults to Assassinate for safety as a fallback if a type is missing.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public MalfAiSabotageType SabotageType = MalfAiSabotageType.Assassinate;
}
