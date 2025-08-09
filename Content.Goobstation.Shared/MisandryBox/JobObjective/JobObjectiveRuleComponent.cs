using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;

namespace Content.Goobstation.Shared.MisandryBox.JobObjective;

[RegisterComponent]
public sealed partial class JobObjectiveRuleComponent : Component
{
    [DataField]
    public List<(EntityUid Mind, MindComponent MindComp)> TrackedMinds = [];
}
