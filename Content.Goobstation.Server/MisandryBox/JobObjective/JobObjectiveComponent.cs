namespace Content.Goobstation.Server.MisandryBox.JobObjective;

/// <summary>
/// Component for jobs that should receive objectives upon spawning
/// </summary>
[RegisterComponent]
public sealed partial class JobObjectiveComponent : Component
{
    /// <summary>
    /// List of objective prototypes to assign to this job
    /// </summary>
    [DataField("objectives", required: true)]
    public List<string> Objectives = new();
}
