// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Wizard.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Wizard.Components;

[RegisterComponent, Access(typeof(WizardRuleSystem))]
public sealed partial class RuleOnWizardDeathRuleComponent : Component
{
    [DataField(required: true)]
    public EntProtoId Rule = default!;
}