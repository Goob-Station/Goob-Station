using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Administration;

[Serializable, NetSerializable]
public sealed class TimeTransferPanelEuiState : EuiStateBase
{
    public bool HasFlag { get; }

    public TimeTransferPanelEuiState(bool hasFlag)
    {
        HasFlag = hasFlag;
    }
}

[Serializable, NetSerializable]
public sealed class TimeTransferEuiMessage : EuiMessageBase
{
    public string PlayerId { get; }
    public List<TimeTransferData> TimeData { get; }

    public bool Overwrite { get; }

    public TimeTransferEuiMessage(string playerId, List<TimeTransferData> timeData, bool overwrite)
    {
        PlayerId = playerId;
        TimeData = timeData;
        Overwrite = overwrite;
    }
}

[Serializable, NetSerializable]
public sealed class TimeTransferWarningEuiMessage : EuiMessageBase
{
    public string Message { get; }
    public Color WarningColor { get; }

    public TimeTransferWarningEuiMessage(string message, Color color)
    {
        Message = message;
        WarningColor = color;
    }
}

[DataDefinition]
[Serializable, NetSerializable]
public partial record struct TimeTransferData
{
    [DataField]
    public string TimeString { get; init; }

    [DataField]
    public string PlaytimeTracker { get; init; }

    public TimeTransferData(string tracker, string timeString)
    {
        PlaytimeTracker = tracker;
        TimeString = timeString;
    }
}
