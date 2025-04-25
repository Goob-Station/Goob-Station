using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Obsessed;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ObsessionTargetComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public int Id = 0;
}
