using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.Swap;

[RegisterComponent, NetworkedComponent]
public sealed partial class SwapSpellComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? SecondaryTarget;
}
