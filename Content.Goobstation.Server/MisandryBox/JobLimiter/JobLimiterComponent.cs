namespace Content.Goobstation.Server.MisandryBox.JobLimiter;

/// <summary>
/// Used on station entities that limit the amount of "bound" jobs are available per "controlling" jobs taken.
/// </summary>
[RegisterComponent]
public sealed partial class JobLimiterComponent : Component
{
    public bool Active = false;

    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<string, int?> JobCounts = new();
}
