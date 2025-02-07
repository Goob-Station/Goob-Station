using Content.Server.StationEvents.Events;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(TidemindDevelopRule))]
public sealed partial class TidemindDevelopRuleComponent : Component
{
    [DataField]
    public List<ProtoId<JobPrototype>> AffectedJobs = new();
}
