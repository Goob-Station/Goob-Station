using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.NTR;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class NtrAccountClientComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Balance;
}

