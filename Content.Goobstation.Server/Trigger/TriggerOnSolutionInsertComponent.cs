namespace Content.Goobstation.Server.Trigger;

/// <summary>
///     sends a trigger if item injected into a container contains an ammount of a solution.
/// </summary>
[RegisterComponent]
public sealed partial class TriggerOnSolutionInsertComponent : Component
{
    [DataField]
    public string SolutionName = "Unkown";
    [DataField]
    public float? MinAmount;    // Dos not trigger in found ammount found is below
    [DataField]
    public float? MaxAmount;    // Dos not trigger in found ammount found is Above
    [DataField]
    public string? ContainerName = null;
    [DataField]
    public float Depth = 1;
}
