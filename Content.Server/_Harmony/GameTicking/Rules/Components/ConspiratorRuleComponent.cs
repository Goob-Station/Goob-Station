using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Server._Harmony.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(ConspiratorRuleSystem))]
public sealed partial class ConspiratorRuleComponent : Component
{
    [DataField]
    public EntProtoId? Objective = null;

    [DataField]
    public ProtoId<WeightedRandomPrototype> ObjectiveGroup = "ConspiratorObjectiveGroup";
}
