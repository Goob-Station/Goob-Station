// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Oskarrr.WorkingJoe;

[Serializable, NetSerializable]
public enum WorkingJoeVoiceUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class WorkingJoePlayLineMessage : BoundUserInterfaceMessage
{
    public string LineId;

    public WorkingJoePlayLineMessage(string lineId)
    {
        LineId = lineId;
    }
}

[Serializable, NetSerializable]
public sealed class WorkingJoeVoiceLineData
{
    public string Category = string.Empty;
    public string DisplayName = string.Empty;
    public string LineId = string.Empty;
}
