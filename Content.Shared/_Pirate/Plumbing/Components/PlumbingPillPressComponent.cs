using Robust.Shared.GameStates;

namespace Content.Shared._Pirate.Plumbing.Components;

/// <summary>
///     A plumbing pill press that automatically creates pills or patches
///     from reagents pulled through its inlet network.
///     When the buffer accumulates enough reagents to meet the dosage,
///     it spawns a pill or patch entity on the ground containing those reagents.
///     Supports optional mixing mode with two configurable ratio inlets (E/W)
///     in addition to the normal north inlet.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class PlumbingPillPressComponent : Component
{
    /// <summary>
    ///     Name of the buffer solution that holds incoming reagents.
    /// </summary>
    [DataField]
    public string BufferSolutionName = "buffer";

    /// <summary>
    ///     The dosage per pill/patch in units. Clamped 1–20.
    /// </summary>
    [DataField, AutoNetworkedField]
    public uint Dosage = 10;

    /// <summary>
    ///     Whether we're producing pills or patches.
    /// </summary>
    [DataField, AutoNetworkedField]
    public PillPressOutputMode OutputMode = PillPressOutputMode.Pill;

    /// <summary>
    ///     The pill visual type (0–19). Only relevant for pills.
    /// </summary>
    [DataField, AutoNetworkedField]
    public uint PillType;

    /// <summary>
    ///     Label applied to created pills/patches.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string Label = string.Empty;

    /// <summary>
    ///     Whether the press is enabled and will pull/produce.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    // =========================================================================
    // Mixing mode fields
    // =========================================================================

    /// <summary>
    ///     Whether ratio-controlled mixing is active.
    ///     When enabled, E/W inlets pull proportionally into staging solutions
    ///     and combine into the buffer when targets are met.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool MixingEnabled;

    /// <summary>
    ///     Ratio percentage for the east inlet (0–100).
    /// </summary>
    [DataField, AutoNetworkedField]
    public float InletRatioEast;

    /// <summary>
    ///     Ratio percentage for the west inlet (0–100).
    /// </summary>
    [DataField, AutoNetworkedField]
    public float InletRatioWest;

    /// <summary>
    ///     Name of the east staging solution.
    /// </summary>
    [DataField]
    public string StagingEastSolutionName = "stagingEast";

    /// <summary>
    ///     Name of the west staging solution.
    /// </summary>
    [DataField]
    public string StagingWestSolutionName = "stagingWest";

    /// <summary>
    ///     Node name for the east inlet.
    /// </summary>
    [DataField]
    public string InletEastNodeName = "mixingInletEast";

    /// <summary>
    ///     Node name for the west inlet.
    /// </summary>
    [DataField]
    public string InletWestNodeName = "mixingInletWest";
}
