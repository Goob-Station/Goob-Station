using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Grab;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GrabbingItemComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? GrabbedEntity;

    [DataField]
    public TimeSpan GrabBreakDelay = TimeSpan.FromSeconds(5);
}
