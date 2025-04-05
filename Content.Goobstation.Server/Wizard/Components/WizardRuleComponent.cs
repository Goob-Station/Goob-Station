using Content.Goobstation.Server.Wizard.Systems;

namespace Content.Goobstation.Server.Wizard.Components;

[RegisterComponent, Access(typeof(WizardRuleSystem))]
public sealed partial class WizardRuleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? TargetStation;
}
