using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MisandryBox.JobLimiter;

[Prototype("jobLimitRule")]
public sealed partial class JobLimitRulePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Job that will have its slots limited
    /// </summary>
    [DataField(required: true)]
    public ProtoId<JobPrototype> LimitedJob = default!;

    /// <summary>
    /// Jobs that control the slot limit for the limited job
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<JobPrototype>> ControllingJobs = new();

    /// <summary>
    /// How many limited job slots per controlling job
    /// </summary>
    [DataField]
    public float Ratio = 1.0f;

    /// <summary>
    /// Minimum number of controlling jobs before any limited jobs are allowed
    /// </summary>
    [DataField]
    public int MinimumControlling = 1;

    /// <summary>
    /// Absolute maximum number of limited jobs regardless of ratio
    /// </summary>
    [DataField]
    public int? AbsoluteMax;

    /// <summary>
    /// Priority for applying rules (higher numbers applied first)
    /// </summary>
    [DataField]
    public int Priority = 0;
}
