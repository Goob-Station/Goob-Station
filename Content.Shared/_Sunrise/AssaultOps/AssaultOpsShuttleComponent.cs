using Robust.Shared.GameStates;

namespace Content.Server._Sunrise.AssaultOps;

[RegisterComponent, NetworkedComponent]
public sealed partial class AssaultOpsShuttleComponent : Component
{
    [DataField]
    public EntityUid AssociatedRule;
}
