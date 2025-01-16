using Content.Shared.Silicons.Laws;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Objectives.Components;

[RegisterComponent]
public sealed partial class AILawsUpdatedRequirementComponent : Component
{
    /// <summary>
    ///     The lawset that is needed to complete the objective.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<SiliconLawsetPrototype> Lawset;
}
