using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class AlertCarvingComponent : Component
{
    [DataField]
    public EntityUid? User;
}
