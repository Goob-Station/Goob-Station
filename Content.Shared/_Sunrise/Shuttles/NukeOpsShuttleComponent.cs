using Robust.Shared.GameStates;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class NukeOpsShuttleComponent : Component
{
    [DataField]
    public EntityUid AssociatedRule;
}
