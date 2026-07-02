using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Pirate.Plumbing.Components;

/// <summary>
///     A plumbing synthesizer that generates reagents from electrical power.
///     Drains power from its battery to produce a selected reagent into
///     its buffer. Other machines can pull the generated reagents via
///     the <see cref="PlumbingOutletComponent"/> on this entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class PlumbingSynthesizerComponent : Component
{
    /// <summary>
    ///     Name of the outlet node for the plumbing network.
    /// </summary>
    [DataField]
    public string OutletName = "outlet";

    /// <summary>
    ///     Name of the buffer solution that holds generated reagents.
    /// </summary>
    [DataField]
    public string BufferSolutionName = "buffer";

    /// <summary>
    ///     Available reagents that can be generated.
    ///     Key is reagent prototype ID, value is power drain per unit.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<ReagentPrototype>, float> GeneratableReagents = new();

    /// <summary>
    ///     Currently selected reagent to generate. Null if none selected.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<ReagentPrototype>? SelectedReagent;

    /// <summary>
    ///     Whether the synthesizer is currently enabled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    ///     Charge (in watts) added to the inserted power cell each second while the machine
    ///     has grid power. Recharged in-place here rather than via a <c>Charger</c>, which only
    ///     wakes on cell insert/remove and never resumes for a cell drained inside the machine.
    /// </summary>
    [DataField]
    public float CellRechargeRate = 30f;
}
