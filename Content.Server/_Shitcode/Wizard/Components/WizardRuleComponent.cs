// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class WizardRuleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? TargetStation;
}