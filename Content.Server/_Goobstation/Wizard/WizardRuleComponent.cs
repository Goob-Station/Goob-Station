using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard;

[RegisterComponent, Access(typeof(WizardRuleSystem))]
public sealed partial class WizardRuleComponent : Component
{
    [DataField]
    public ProtoId<NpcFactionPrototype> Faction = "Wizard";

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? TargetStation;
}
