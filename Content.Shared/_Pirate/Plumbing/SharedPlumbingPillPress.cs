using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared._Pirate.Plumbing;

/// <summary>
///     Whether the pill press produces pills or patches.
/// </summary>
[Serializable, NetSerializable]
public enum PillPressOutputMode : byte
{
    Pill,
    Patch,
}

/// <summary>
///     UI key for the plumbing pill press interface.
/// </summary>
[Serializable, NetSerializable]
public enum PlumbingPillPressUiKey : byte
{
    Key,
}

/// <summary>
///     State sent to the client to update the pill press UI.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingPillPressBoundUserInterfaceState : BoundUserInterfaceState
{
    public FixedPoint2 BufferVolume { get; }
    public uint Dosage { get; }
    public PillPressOutputMode OutputMode { get; }
    public uint PillType { get; }
    public string Label { get; }
    public bool Enabled { get; }

    // Mixing mode fields
    public bool MixingEnabled { get; }
    public float InletRatioEast { get; }
    public float InletRatioWest { get; }
    public FixedPoint2 StagingEastVolume { get; }
    public FixedPoint2 StagingWestVolume { get; }

    public PlumbingPillPressBoundUserInterfaceState(
        FixedPoint2 bufferVolume,
        uint dosage,
        PillPressOutputMode outputMode,
        uint pillType,
        string label,
        bool enabled,
        bool mixingEnabled,
        float inletRatioEast,
        float inletRatioWest,
        FixedPoint2 stagingEastVolume,
        FixedPoint2 stagingWestVolume)
    {
        BufferVolume = bufferVolume;
        Dosage = dosage;
        OutputMode = outputMode;
        PillType = pillType;
        Label = label;
        Enabled = enabled;
        MixingEnabled = mixingEnabled;
        InletRatioEast = inletRatioEast;
        InletRatioWest = inletRatioWest;
        StagingEastVolume = stagingEastVolume;
        StagingWestVolume = stagingWestVolume;
    }
}

/// <summary>
///     Message to toggle the pill press on/off.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingPillPressToggleMessage : BoundUserInterfaceMessage
{
    public bool Enabled { get; }

    public PlumbingPillPressToggleMessage(bool enabled)
    {
        Enabled = enabled;
    }
}

/// <summary>
///     Message to set the dosage (1–20).
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingPillPressSetDosageMessage : BoundUserInterfaceMessage
{
    public uint Dosage { get; }

    public PlumbingPillPressSetDosageMessage(uint dosage)
    {
        Dosage = dosage;
    }
}

/// <summary>
///     Message to switch between pill and patch output.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingPillPressSetOutputModeMessage : BoundUserInterfaceMessage
{
    public PillPressOutputMode OutputMode { get; }

    public PlumbingPillPressSetOutputModeMessage(PillPressOutputMode outputMode)
    {
        OutputMode = outputMode;
    }
}

/// <summary>
///     Message to set the pill type visual (0–19).
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingPillPressSetPillTypeMessage : BoundUserInterfaceMessage
{
    public uint PillType { get; }

    public PlumbingPillPressSetPillTypeMessage(uint pillType)
    {
        PillType = pillType;
    }
}

/// <summary>
///     Message to set the output label for created pills/patches.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingPillPressSetLabelMessage : BoundUserInterfaceMessage
{
    public string Label { get; }

    public PlumbingPillPressSetLabelMessage(string label)
    {
        Label = label;
    }
}

/// <summary>
///     Message to toggle mixing mode on/off.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingPillPressSetMixingMessage : BoundUserInterfaceMessage
{
    public bool MixingEnabled { get; }

    public PlumbingPillPressSetMixingMessage(bool mixingEnabled)
    {
        MixingEnabled = mixingEnabled;
    }
}

/// <summary>
///     Identifies which mixing inlet is being configured.
/// </summary>
[Serializable, NetSerializable]
public enum PillPressInlet : byte
{
    East,
    West,
}

/// <summary>
///     Message to set the ratio for a mixing inlet.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingPillPressSetInletRatioMessage : BoundUserInterfaceMessage
{
    public PillPressInlet Inlet { get; }
    public float Ratio { get; }

    public PlumbingPillPressSetInletRatioMessage(PillPressInlet inlet, float ratio)
    {
        Inlet = inlet;
        Ratio = ratio;
    }
}
