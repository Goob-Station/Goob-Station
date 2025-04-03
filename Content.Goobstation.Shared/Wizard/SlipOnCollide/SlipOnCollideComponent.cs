using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wizard.SlipOnCollide;

[RegisterComponent, NetworkedComponent]
public sealed partial class SlipOnCollideComponent : Component
{
    [DataField]
    public bool Force = true;
}
