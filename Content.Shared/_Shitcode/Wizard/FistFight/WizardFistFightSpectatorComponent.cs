using Robust.Shared.Map;

namespace Content.Shared._Goobstation.Wizard.FistFight;

[RegisterComponent]
public sealed partial class WizardFistFightSpectatorComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityCoordinates Home;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool WasPacified;
}
