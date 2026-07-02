using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Pirate.Plumbing.Components;

/// <summary>
///     A plumbing filter that separates reagents into two outputs.
///     Pulls reagents from the inlet into a buffer, then restricts which reagents
///     can be pulled from each outlet via <see cref="PlumbingPullAttemptEvent"/>.
///     Filter outlet only provides reagents in the filter list.
///     Passthrough outlet only provides reagents NOT in the filter list.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class PiratePlumbingFilterComponent : Component
{
    public const int MaxFilteredReagents = 4;

    /// <summary>
    ///     Name of the filter outlet node - only filtered reagents can be pulled here.
    /// </summary>
    [DataField]
    public string FilterNodeName = "outletFilter";

    /// <summary>
    ///     Name of the passthrough outlet node - only non-filtered reagents can be pulled here.
    /// </summary>
    [DataField]
    public string PassthroughNodeName = "outletPassthrough";

    /// <summary>
    ///     Name of the solution lane that holds filtered reagents.
    /// </summary>
    [DataField]
    public string FilteredSolutionName = "bufferFiltered";

    /// <summary>
    ///     Name of the solution lane that holds passthrough reagents.
    /// </summary>
    [DataField]
    public string PassthroughSolutionName = "buffer";

    /// <summary>
    ///     Whether the filter is currently enabled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    ///     The reagent IDs to filter out to the filter port.
    ///     Multiple reagents can be filtered simultaneously.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<ReagentPrototype>> FilteredReagents = new();
}
