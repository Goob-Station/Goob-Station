using Content.Shared.Atmos;
using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Pirate.Plumbing.Components;

/// <summary>
///     A plumbing reactor that accumulates reagents and triggers chemical reactions.
///     Pulls specific reagents from the inlet until target quantities are met,
///     then triggers reactions in the buffer at a configurable temperature.
///     Products are moved to an output solution for other machines to pull.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class PlumbingReactorComponent : Component
{
    /// <summary>
    ///     Name of the inlet node for pulling reagents from the network.
    /// </summary>
    [DataField]
    public string InletName = "inlet";

    /// <summary>
    ///     Name of the buffer solution that holds reagents waiting to react.
    /// </summary>
    [DataField]
    public string BufferSolutionName = "buffer";

    /// <summary>
    ///     Name of the output solution that holds reacted products.
    /// </summary>
    [DataField]
    public string OutputSolutionName = "output";

    /// <summary>
    ///     Amount to pull from tanks per update.
    /// </summary>
    [DataField]
    public FixedPoint2 TransferAmount = FixedPoint2.New(20);

    /// <summary>
    ///     Whether the reactor is currently enabled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    ///     The reagent targets to accumulate. Key is reagent prototype ID, value is target quantity.
    ///     When all targets are met, the buffer triggers reactions and moves products to output.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> ReagentTargets = new();

    /// <summary>
    ///     Target temperature for the buffer solution in Kelvin.
    ///     The reactor will heat or cool the buffer to reach this temperature.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float TargetTemperature = Atmospherics.T20C;

    /// <summary>
    ///     Maximum heat transfer rate when powered, in watts.
    /// </summary>
    [DataField]
    public float HeatTransferPower = 500f;
}
