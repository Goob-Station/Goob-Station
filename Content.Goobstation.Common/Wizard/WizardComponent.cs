using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Wizard;

[RegisterComponent, NetworkedComponent]
public sealed partial class WizardComponent : Component
{
    [DataField]
    public string StatusIcon = "WizardFaction";
}
