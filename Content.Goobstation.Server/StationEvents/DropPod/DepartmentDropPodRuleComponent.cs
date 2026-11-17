using Content.Shared.Dataset;
using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Radio;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.StationEvents.DropPod;

[RegisterComponent, Access(typeof(DepartmentDropPodRuleSystem))]
public sealed partial class DepartmentDropPodRuleComponent : Component
{
    [DataField(required: true)]
    public ProtoId<DepartmentPrototype> Department;

    [DataField]
    public List<ProtoId<JobPrototype>> TargetJobs = new();

    [DataField(required: true)]
    public ProtoId<RadioChannelPrototype> Channel;

    [DataField(required: true)]
    public EntityTableSelector Contents = default!;

    [DataField(required: true)]
    public LocId Problem;

    [DataField(required: true)]
    public LocId Supplies;

    [DataField]
    public List<LocId> Messages = new()
    {
        "department-drop-pod-message-1",
        "department-drop-pod-message-2",
        "department-drop-pod-message-3",
        "department-drop-pod-message-4",
    };

    [DataField]
    public EntProtoId Speaker = "DropPodCommanderSpeaker";

    [DataField]
    public LocId SpeakerName = "department-drop-pod-commander-name";

    [DataField]
    public ProtoId<LocalizedDatasetPrototype> FirstNames = "NamesFirst";

    [DataField]
    public ProtoId<LocalizedDatasetPrototype> LastNames = "NamesLast";
}
