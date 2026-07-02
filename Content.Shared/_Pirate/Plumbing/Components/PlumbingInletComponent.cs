using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared._Pirate.Plumbing.Components;

/// <summary>
///     A plumbing inlet that pulls reagents from the network.
///     Actively pulls reagents from its inlet nodes each update tick into the specified solution.
///     Each inlet node connects to its own plumbing network, preventing network bridging
///     through multi-connector machines.
///     Other machines can pull from this entity via <see cref="PlumbingOutletComponent"/>.
/// </summary>
[RegisterComponent]
public sealed partial class PlumbingInletComponent : Component
{
    /// <summary>
    ///     The name of the solution on this entity to pull reagents into.
    /// </summary>
    [DataField]
    public string SolutionName = "tank";

    /// <summary>
    ///     The names of the inlet nodes to pull from.
    ///     Each node should be a single-direction PlumbingNode so that each direction
    ///     has its own isolated network.
    /// </summary>
    [DataField]
    public List<string> InletNames = new() { "inlet" };

    /// <summary>
    ///     Amount to transfer per update, shared across all inlets.
    /// </summary>
    [DataField]
    public FixedPoint2 TransferAmount = FixedPoint2.New(20);

    /// <summary>
    ///     Round-robin indices for fair outlet selection.
    ///     Tracks which outlet to start from when pulling from multiple sources on each network.
    /// </summary>
    public Dictionary<string, int> RoundRobinIndices = new();
}
