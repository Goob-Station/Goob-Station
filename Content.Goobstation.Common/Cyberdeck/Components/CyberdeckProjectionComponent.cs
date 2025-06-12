using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Cyberdeck.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CyberdeckProjectionComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid? RemoteEntity;
}
