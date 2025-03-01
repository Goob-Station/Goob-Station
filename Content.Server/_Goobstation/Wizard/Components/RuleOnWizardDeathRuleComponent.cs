using Content.Server._Goobstation.Wizard.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent, Access(typeof(WizardRuleSystem))]
public sealed partial class RuleOnWizardDeathRuleComponent : Component
{
    [DataField(required: true)]
    public EntProtoId Rule = default!;
}
