using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.ArcaneBarrage;

[RegisterComponent, NetworkedComponent]
public sealed partial class ArcaneBarrageComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public bool Unremoveable = true;
}
