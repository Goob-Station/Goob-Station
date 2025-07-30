using Robust.Shared.Prototypes;

namespace Content.Pirate.Common.AlternativeJobs;

[Prototype]
public sealed partial class AlternativeJobPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;


    /// <summary>
    /// The name of the job used in ID cards on <see cref="RulePlayerJobsAssignedEvent"/>
    /// </summary>
    [DataField(required: true)]
    public string JobName { get; set; } = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedJobName => Loc.GetString(JobName);

    [DataField(required: true)]
    public string JobDescription { get; set; } = default!;

    [DataField(required: false)]
    public string? JobIconProtoId { get; set; } = null;

    [DataField(required: true)]
    public string ParentJobId { get; set; } = default!;
}
