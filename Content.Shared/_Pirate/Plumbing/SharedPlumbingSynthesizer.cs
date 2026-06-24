using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared._Pirate.Plumbing;

/// <summary>
///     UI key for the plumbing synthesizer interface.
/// </summary>
[Serializable, NetSerializable]
public enum PlumbingSynthesizerUiKey : byte
{
    Key,
}

/// <summary>
///     State sent to the client to update the synthesizer UI.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingSynthesizerBoundUserInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    ///     Available reagents that can be generated.
    ///     Key is reagent prototype ID, value is power drain per unit.
    /// </summary>
    public Dictionary<string, float> GeneratableReagents { get; }

    /// <summary>
    ///     Currently selected reagent ID, or null if none selected.
    /// </summary>
    public string? SelectedReagent { get; }

    /// <summary>
    ///     Current contents of the buffer.
    /// </summary>
    public Dictionary<string, FixedPoint2> BufferContents { get; }

    /// <summary>
    ///     Whether the synthesizer is enabled.
    /// </summary>
    public bool Enabled { get; }

    /// <summary>
    ///     Current battery charge (0-1 ratio).
    /// </summary>
    public float BatteryCharge { get; }

    public PlumbingSynthesizerBoundUserInterfaceState(
        Dictionary<string, float> generatableReagents,
        string? selectedReagent,
        Dictionary<string, FixedPoint2> bufferContents,
        bool enabled,
        float batteryCharge)
    {
        GeneratableReagents = generatableReagents;
        SelectedReagent = selectedReagent;
        BufferContents = bufferContents;
        Enabled = enabled;
        BatteryCharge = batteryCharge;
    }
}

/// <summary>
///     Message to toggle the synthesizer on/off.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingSynthesizerToggleMessage : BoundUserInterfaceMessage
{
    public bool Enabled { get; }

    public PlumbingSynthesizerToggleMessage(bool enabled)
    {
        Enabled = enabled;
    }
}

/// <summary>
///     Message to select which reagent to generate.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingSynthesizerSelectReagentMessage : BoundUserInterfaceMessage
{
    /// <summary>
    ///     The reagent ID to select, or null to deselect.
    /// </summary>
    public string? ReagentId { get; }

    public PlumbingSynthesizerSelectReagentMessage(string? reagentId)
    {
        ReagentId = reagentId;
    }
}
