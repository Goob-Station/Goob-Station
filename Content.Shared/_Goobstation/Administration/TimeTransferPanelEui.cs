using Content.Shared.Eui;
using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Administration;

[Serializable, NetSerializable]
public sealed class TimeTransferPanelEuiState : EuiStateBase
{
    public bool HasFlag { get; set; }

    public TimeTransferPanelEuiState(bool hasFlag)
    {
        HasFlag = hasFlag;
    }
}

[Serializable, NetSerializable]
public sealed class TimeTransferEuiMessage : EuiMessageBase
{
    public string Player { get; }
    public float Time { get; }

    public TimeTransferEuiMessage(string player, float time)
    {
        Player = player;
        Time = time;
    }
}
