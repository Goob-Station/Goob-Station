using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Doodon.Objectives;

/// <summary>
/// Objective condition: keep at least N worker doodons alive.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class KeepWorkersAliveConditionComponent : Component
{
    /// <summary>
    /// Which prototype counts as a "worker".
    /// </summary>
    [DataField(required: true)]
    public EntProtoId WorkerPrototype = default!;
}
