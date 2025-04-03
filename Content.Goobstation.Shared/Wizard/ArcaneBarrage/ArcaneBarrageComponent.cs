using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wizard.ArcaneBarrage;

[RegisterComponent, NetworkedComponent]
public sealed partial class ArcaneBarrageComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public bool Unremoveable = true;
}
