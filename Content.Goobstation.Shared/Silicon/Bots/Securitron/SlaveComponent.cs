using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Silicon.Bots.Securitron;

/// <summary>
/// Designate's a robot's master.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlaveComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? MasterEntity { get; set; }

    /// <summary>
    /// Should the slave be patrolling?
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool IsPatrolling;
}
