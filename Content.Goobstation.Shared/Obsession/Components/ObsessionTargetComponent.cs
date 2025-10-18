using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Obsession;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ObsessionTargetComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public int Id = 0;
}
