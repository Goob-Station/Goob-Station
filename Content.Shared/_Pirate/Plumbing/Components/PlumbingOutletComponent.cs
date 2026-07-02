using Robust.Shared.GameStates;

namespace Content.Shared._Pirate.Plumbing.Components;

/// <summary>
///     Marks an entity as a reagent source on the plumbing network.
///     <see cref="PlumbingPullSystem"/> discovers entities with this component
///     when other machines request reagents from the network.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PlumbingOutletComponent : Component
{
    /// <summary>
    ///     The name of the solution to provide to the network.
    /// </summary>
    [DataField]
    public string SolutionName = "tank";

    /// <summary>
    ///     The outlet node names that serve this solution.
    ///     Defaults to a single "outlet" node. Devices with multiple outlets
    ///     (e.g. the filter) can list several names.
    /// </summary>
    [DataField]
    public List<string> OutletNames = new() { "outlet" };

    /// <summary>
    ///     If true, this outlet can be pulled from. If false, it's blocked.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    ///     If set, look for the solution on the entity in this container slot instead of on this entity.
    ///     Useful for machines like dispensers where the solution is in a beaker.
    /// </summary>
    [DataField]
    public string? ContainerSlotId;

    /// <summary>
    ///     If true, configured outlet nodes on this machine are internally linked in the
    ///     plumbing graph, allowing attached duct networks to bridge through the machine.
    ///     Visual connector state and per-edge layer limits are unchanged.
    /// </summary>
    [DataField]
    public bool ConnectedOutlets;
}
