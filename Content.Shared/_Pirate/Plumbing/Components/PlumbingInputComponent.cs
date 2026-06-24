namespace Content.Shared._Pirate.Plumbing.Components;

/// <summary>
///     A plumbing input that players can pour reagents into using containers.
///     Paired with <see cref="PlumbingOutletComponent"/> to allow other machines
///     to pull reagents from its solution via the plumbing network.
/// </summary>
[RegisterComponent]
public sealed partial class PlumbingInputComponent : Component
{
    /// <summary>
    ///     The name of the solution on this entity that receives input.
    /// </summary>
    [DataField]
    public string SolutionName = "input";
}
