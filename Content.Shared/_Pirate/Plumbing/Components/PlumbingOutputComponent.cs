namespace Content.Shared._Pirate.Plumbing.Components;

/// <summary>
///     A plumbing output that players can draw reagents from using containers.
///     Pulling from the network is handled by <see cref="PlumbingInletComponent"/>.
/// </summary>
[RegisterComponent]
public sealed partial class PlumbingOutputComponent : Component
{
    /// <summary>
    ///     The name of the solution on this entity that players can draw from.
    /// </summary>
    [DataField]
    public string SolutionName = "output";
}
